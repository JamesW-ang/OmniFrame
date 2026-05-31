using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OmniFrame.Common;
using OmniFrame.Core;

namespace RemoteMonitor
{
    /// <summary>
    /// Web API控制器类
    /// 设计介绍：
    /// 1. 实现RESTful API接口，支持HTTP请求和响应
    /// 2. 支持GET、POST、PUT、DELETE等HTTP方法
    /// 3. 实现JSON格式的数据交换
    /// 4. 提供统一的错误处理和响应格式
    /// 5. 支持跨域请求（CORS）
    /// 6. 实现基本的身份验证和权限控制
    /// 7. 线程安全设计，支持多线程并发访问
        /// </summary>
    [Obsolete("Hand-rolled HTTP server. Consider migrating to OWIN/Katana for production.")]
    public class WebApiController : IDisposable
    {
        private HttpListener _httpListener;
        private int _port;
        private bool _isRunning;
        private IRemoteMonitorManager _remoteMonitorManager;
        private Dictionary<string, List<DateTime>> _rateLimitCache;
        private readonly object _rateLimitLock = new object();
        private Timer _rateLimitCleanupTimer;
        private List<string> _corsWhitelist;
        private string _jwtSecretKey;
        private readonly IConfigManager _configManager;
        private readonly ISystemManager _systemManager;

        /// <summary>速率限制窗口（分钟）</summary>
        private const int RateLimitWindowMinutes = 1;

        /// <summary>速率限制上限（请求/窗口）</summary>
        private const int RateLimitMaxRequests = 100;

        /// <summary>过期 IP 记录清理间隔（分钟）</summary>
        private const int RateLimitCleanupIntervalMinutes = 5;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port">监听端口</param>
        public WebApiController(int port, IRemoteMonitorManager remoteMonitorManager, IConfigManager configManager, ISystemManager systemManager)
        {
            _port = port;
            _remoteMonitorManager = remoteMonitorManager;
            _configManager = configManager;
            _systemManager = systemManager;
            _rateLimitCache = new Dictionary<string, List<DateTime>>();

            // 从配置文件读取CORS白名单
            var systemConfig = _configManager.GetConfig<OmniFrame.Core.SystemConfig>("SystemCfg.xml", new OmniFrame.Core.SystemConfig());
            if (systemConfig.NetworkConfig != null && systemConfig.NetworkConfig.CorsWhitelist != null && systemConfig.NetworkConfig.CorsWhitelist.Count > 0)
            {
                _corsWhitelist = systemConfig.NetworkConfig.CorsWhitelist;
            }
            else
            {
                // 默认只允许本机访问
                _corsWhitelist = new List<string> { "http://localhost", "http://127.0.0.1", "http://localhost:8080", "http://127.0.0.1:8080" };
            }
            
            _jwtSecretKey = GetJwtSecretKey();
        }

        /// <summary>
        /// 获取JWT密钥
        /// </summary>
        private string GetJwtSecretKey()
        {
            // 优先从环境变量获取密钥
            string envKey = Environment.GetEnvironmentVariable("AUTOFRAME_JWT_SECRET");
            if (!string.IsNullOrEmpty(envKey))
            {
                return envKey;
            }

            // 如果环境变量不存在，拒绝启动
            throw new InvalidOperationException("环境变量 AUTOFRAME_JWT_SECRET 未设置，拒绝启动。请设置此环境变量后重新运行。");
        }

        /// <summary>
        /// 生成JWT Token
        /// </summary>
        private string GenerateToken(string username, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "OmniFrame",
                audience: "AutoFrameClients",
                claims: claims,
                expires: DateTime.Now.AddHours(2), // 有效期2小时
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// 验证JWT Token
        /// </summary>
        private bool ValidateToken(string token, out ClaimsPrincipal principal)
        {
            principal = null;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "OmniFrame",
                    ValidAudience = "AutoFrameClients",
                    IssuerSigningKey = key
                };

                principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Token验证失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 检查请求速率限制。
        /// 每个 IP 每分钟最多 {RateLimitMaxRequests} 次请求。
        /// 自动清理当前窗口外的旧记录。
        /// </summary>
        private bool CheckRateLimit(string clientIp)
        {
            lock (_rateLimitLock)
            {
                if (!_rateLimitCache.TryGetValue(clientIp, out var timestamps))
                {
                    timestamps = new List<DateTime>();
                    _rateLimitCache[clientIp] = timestamps;
                }

                var now = DateTime.Now;
                var windowStart = now.AddMinutes(-RateLimitWindowMinutes);

                // 移除窗口外的旧记录
                timestamps.RemoveAll(t => t < windowStart);

                if (timestamps.Count >= RateLimitMaxRequests)
                    return false;

                timestamps.Add(now);
                return true;
            }
        }

        /// <summary>
        /// 定期清理长期不活跃的 IP 记录，防止内存泄漏。
        /// 由后台 Timer 每 {RateLimitCleanupIntervalMinutes} 分钟调用一次。
        /// </summary>
        private void CleanupStaleRateLimitEntries()
        {
            try
            {
                lock (_rateLimitLock)
                {
                    var cutoff = DateTime.Now.AddMinutes(-RateLimitCleanupIntervalMinutes);
                    var staleKeys = _rateLimitCache
                        .Where(kv => kv.Value.Count == 0 || kv.Value.All(t => t < cutoff))
                        .Select(kv => kv.Key)
                        .ToList();

                    foreach (var key in staleKeys)
                        _rateLimitCache.Remove(key);

                    if (staleKeys.Count > 0)
                        Logger.Info($"速率限制缓存清理: 移除 {staleKeys.Count} 个过期 IP 记录, 剩余 {_rateLimitCache.Count}");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning("速率限制缓存清理异常", ex);
            }
        }

        /// <summary>
        /// 检查CORS白名单
        /// </summary>
        private bool IsCorsAllowed(string origin)
        {
            return _corsWhitelist.Contains(origin);
        }

        /// <summary>
        /// 启动Web API服务器
        /// </summary>
        /// <returns>是否启动成功</returns>
        public bool Start()
        {
            try
            {
                if (_isRunning)
                {
                    Logger.Warning("Web API服务器已在运行中");
                    return false;
                }

                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add($"http://+:{_port}/");
                _httpListener.Start();
                _isRunning = true;

                Logger.Info($"Web API服务器启动成功，监听端口: {_port}");

                // 启动速率限制缓存清理定时器（每 5 分钟）
                _rateLimitCleanupTimer = new Timer(
                    _ => CleanupStaleRateLimitEntries(),
                    null,
                    TimeSpan.FromMinutes(RateLimitCleanupIntervalMinutes),
                    TimeSpan.FromMinutes(RateLimitCleanupIntervalMinutes));

                Task.Run(() => HandleRequestsAsync());

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Web API服务器启动失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 停止Web API服务器
        /// </summary>
        public void Stop()
        {
            try
            {
                if (!_isRunning)
                    return;

                // 停止速率限制清理定时器
                _rateLimitCleanupTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                _rateLimitCleanupTimer?.Dispose();
                _rateLimitCleanupTimer = null;

                _httpListener?.Stop();
                _isRunning = false;
                Logger.Info("Web API服务器已停止");
            }
            catch (Exception ex)
            {
                Logger.Error("Web API服务器停止失败", ex);
            }
        }

        public void Dispose()
        {
            Stop();
            _httpListener?.Close();
            _rateLimitCleanupTimer?.Dispose();
        }

        /// <summary>
        /// 处理HTTP请求
        /// </summary>
        private async Task HandleRequestsAsync()
        {
            try
            {
                while (_isRunning)
                {
                    HttpListenerContext context = await _httpListener.GetContextAsync();
                    _ = Task.Run(async () => await ProcessRequestAsync(context));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("处理HTTP请求失败", ex);
            }
        }

        /// <summary>
        /// 处理单个HTTP请求
        /// </summary>
        /// <param name="context">HTTP监听器上下文</param>
        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            string requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            try
            {
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                // Set Content-Type header for all responses
                response.ContentType = "application/json; charset=utf-8";
                response.AddHeader("X-Request-Id", requestId);

                // 获取客户端IP地址
                string clientIp = request.RemoteEndPoint.Address.ToString();

                // 检查请求速率限制
                if (!CheckRateLimit(clientIp))
                {
                    Logger.Warning($"[{requestId}] Rate limit exceeded for {clientIp}");
                    SendErrorResponse(response, 429, "Too Many Requests");
                    return;
                }

                // 处理CORS请求
                string origin = request.Headers.Get("Origin");
                if (!string.IsNullOrEmpty(origin))
                {
                    if (IsCorsAllowed(origin))
                    {
                        response.AddHeader("Access-Control-Allow-Origin", origin);
                    }
                    else
                    {
                        response.AddHeader("Access-Control-Allow-Origin", "");
                    }
                }
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");

                // 处理OPTIONS请求
                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 200;
                    response.Close();
                    return;
                }

                // 解析URL路径
                string path = request.Url.AbsolutePath;
                Logger.Info($"[{requestId}] HTTP {request.HttpMethod} {path} from {clientIp}");

                // 对于API端点，验证JWT Token
                if (path.StartsWith("/api/"))
                {
                    string authorizationHeader = request.Headers.Get("Authorization");
                    if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
                    {
                        Logger.Warning($"[{requestId}] Missing or invalid Authorization header for {path}");
                        SendErrorResponse(response, 401, "Unauthorized: Missing or invalid token");
                        return;
                    }

                    string token = authorizationHeader.Substring(7); // 移除 "Bearer " 前缀
                    if (!ValidateToken(token, out _))
                    {
                        Logger.Warning($"[{requestId}] Invalid JWT token for {path}");
                        SendErrorResponse(response, 401, "Unauthorized: Invalid token");
                        return;
                    }
                }

                // 路由处理 - each route wrapped in try-catch
                object result = null;
                int statusCode = 200;

                switch (path)
                {
                    case "/api/status":
                        try
                        {
                            result = _remoteMonitorManager.GetSystemStatus();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"[{requestId}] /api/status failed", ex);
                            statusCode = 500;
                            result = new { error = "Internal server error", requestId };
                        }
                        break;

                    case "/api/devices":
                        try
                        {
                            result = _remoteMonitorManager.GetDeviceStatus();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"[{requestId}] /api/devices failed", ex);
                            statusCode = 500;
                            result = new { error = "Internal server error", requestId };
                        }
                        break;

                    case "/api/command":
                        if (request.HttpMethod != "POST")
                        {
                            statusCode = 405;
                            result = new { error = "Method Not Allowed" };
                            break;
                        }
                        try
                        {
                            string requestBody = await ReadRequestBodyAsync(request);
                            if (string.IsNullOrWhiteSpace(requestBody))
                            {
                                statusCode = 400;
                                result = new { error = "Request body is required" };
                                break;
                            }
                            RemoteCommand command = JsonConvert.DeserializeObject<RemoteCommand>(requestBody);
                            if (command == null || string.IsNullOrWhiteSpace(command.CommandType))
                            {
                                statusCode = 400;
                                result = new { error = "Invalid command payload" };
                                break;
                            }
                            result = _remoteMonitorManager.ExecuteCommand(command);
                        }
                        catch (JsonException ex)
                        {
                            Logger.Error($"[{requestId}] /api/command JSON parse failed", ex);
                            statusCode = 400;
                            result = new { error = "Invalid JSON in request body" };
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"[{requestId}] /api/command failed", ex);
                            statusCode = 500;
                            result = new { error = "Internal server error", requestId };
                        }
                        break;

                    case "/api/login":
                        if (request.HttpMethod != "POST")
                        {
                            statusCode = 405;
                            result = new { error = "Method Not Allowed" };
                            break;
                        }
                        try
                        {
                            string requestBody = await ReadRequestBodyAsync(request);
                            if (string.IsNullOrWhiteSpace(requestBody))
                            {
                                statusCode = 400;
                                result = new { error = "Request body is required. Provide username and password." };
                                break;
                            }
                            var loginData = JsonConvert.DeserializeObject<dynamic>(requestBody);
                            string username = (string)loginData?.username;
                            string password = (string)loginData?.password;

                            if (string.IsNullOrWhiteSpace(username))
                            {
                                statusCode = 400;
                                result = new { error = "Username is required" };
                                break;
                            }
                            if (string.IsNullOrWhiteSpace(password))
                            {
                                statusCode = 400;
                                result = new { error = "Password is required" };
                                break;
                            }

                            if (_systemManager == null || _systemManager.UserMgr == null)
                            {
                                statusCode = 500;
                                result = new { error = "User management system not available" };
                                break;
                            }

                            var loginResult = _systemManager.UserMgr.Login(username, password);
                            if (loginResult.Success && loginResult.User != null)
                            {
                                string role = loginResult.User.Level.ToString();
                                string token = GenerateToken(username, role);
                                result = new { token = token, username = username, role = role };
                                Logger.Info($"[{requestId}] User logged in: {username}");
                            }
                            else
                            {
                                Logger.Warning($"[{requestId}] Login failed for user: {username}");
                                statusCode = 401;
                                result = new { error = "Invalid username or password" };
                            }
                        }
                        catch (JsonException ex)
                        {
                            Logger.Error($"[{requestId}] /api/login JSON parse failed", ex);
                            statusCode = 400;
                            result = new { error = "Invalid JSON in request body" };
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"[{requestId}] /api/login failed", ex);
                            statusCode = 500;
                            result = new { error = "Internal server error", requestId };
                        }
                        break;

                    case "/api/history/alarm":
                        if (request.HttpMethod != "GET")
                        {
                            statusCode = 405;
                            result = new { error = "Method Not Allowed" };
                            break;
                        }
                        try
                        {
                            result = await GetAlarmHistoryAsync(request);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"[{requestId}] /api/history/alarm failed", ex);
                            statusCode = 500;
                            result = new { error = "Internal server error", requestId };
                        }
                        break;

                    case "/api/history/production":
                        if (request.HttpMethod != "GET")
                        {
                            statusCode = 405;
                            result = new { error = "Method Not Allowed" };
                            break;
                        }
                        try
                        {
                            result = await GetProductionHistoryAsync(request);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"[{requestId}] /api/history/production failed", ex);
                            statusCode = 500;
                            result = new { error = "Internal server error", requestId };
                        }
                        break;

                    case "/api/system/health":
                        if (request.HttpMethod != "GET")
                        {
                            statusCode = 405;
                            result = new { error = "Method Not Allowed" };
                            break;
                        }
                        try
                        {
                            result = GetSystemHealthAsync();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"[{requestId}] /api/system/health failed", ex);
                            statusCode = 500;
                            result = new { error = "Internal server error", requestId };
                        }
                        break;

                    default:
                        statusCode = 404;
                        result = new { error = "Not Found", requestId };
                        break;
                }

                // 发送响应
                Logger.Info($"[{requestId}] Response: {statusCode}");
                SendResponse(response, result, statusCode);
            }
            catch (HttpListenerException ex)
            {
                Logger.Error($"[{requestId}] HTTP listener error", ex);
                SendErrorResponse(context.Response, 500, $"Internal Server Error: {ex.Message}");
            }
            catch (IOException ex)
            {
                Logger.Error($"[{requestId}] IO error", ex);
                SendErrorResponse(context.Response, 500, $"Internal Server Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[{requestId}] Unhandled error", ex);
                SendErrorResponse(context.Response, 500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// 读取请求体
        /// </summary>
        /// <param name="request">HTTP请求</param>
        /// <returns>请求体内容</returns>
        private async Task<string> ReadRequestBodyAsync(HttpListenerRequest request)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <summary>
        /// 发送响应
        /// </summary>
        /// <param name="response">HTTP响应</param>
        /// <param name="data">响应数据</param>
        /// <param name="statusCode">状态码</param>
        private void SendResponse(HttpListenerResponse response, object data, int statusCode)
        {
            try
            {
                response.StatusCode = statusCode;
                response.ContentType = "application/json; charset=utf-8";

                string jsonResponse = JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                });

                byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();

                Logger.Info($"发送HTTP响应，状态码: {statusCode}");
            }
            catch (HttpListenerException ex)
            {
                Logger.Error("发送HTTP响应失败(监听器错误)", ex);
            }
            catch (IOException ex)
            {
                Logger.Error("发送HTTP响应失败(IO错误)", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("发送HTTP响应失败", ex);
            }
        }

        /// <summary>
        /// 发送错误响应
        /// </summary>
        /// <param name="response">HTTP响应</param>
        /// <param name="statusCode">状态码</param>
        /// <param name="message">错误消息</param>
        private void SendErrorResponse(HttpListenerResponse response, int statusCode, string message)
        {
            try
            {
                response.StatusCode = statusCode;
                response.ContentType = "application/json; charset=utf-8";

                var errorResponse = new
                {
                    error = message,
                    timestamp = DateTime.Now
                };

                string jsonResponse = JsonConvert.SerializeObject(errorResponse, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                });

                byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();

                Logger.Error($"发送HTTP错误响应，状态码: {statusCode}，消息: {message}");
            }
            catch (HttpListenerException ex)
            {
                Logger.Error("发送HTTP错误响应失败(监听器错误)", ex);
            }
            catch (IOException ex)
            {
                Logger.Error("发送HTTP错误响应失败(IO错误)", ex);
            }
            catch (Exception ex)
            {
                Logger.Error("发送HTTP错误响应失败", ex);
            }
        }

        /// <summary>
        /// 获取报警历史
        /// </summary>
        /// <param name="request">HTTP请求</param>
        /// <returns>报警历史数据</returns>
        private async Task<object> GetAlarmHistoryAsync(HttpListenerRequest request)
        {
            try
            {
                var systemManager = _systemManager;
                if (systemManager == null || systemManager.AlarmMgr == null)
                {
                    return new { error = "Alarm management system not available" };
                }

                // 解析查询参数
                string startDateStr = request.QueryString["startDate"];
                string endDateStr = request.QueryString["endDate"];
                string levelStr = request.QueryString["level"];

                DateTime? startDate = null;
                DateTime? endDate = null;
                int? level = null;

                if (!string.IsNullOrEmpty(startDateStr))
                {
                    DateTime.TryParse(startDateStr, out var parsedStartDate);
                    startDate = parsedStartDate;
                }

                if (!string.IsNullOrEmpty(endDateStr))
                {
                    DateTime.TryParse(endDateStr, out var parsedEndDate);
                    endDate = parsedEndDate;
                }

                if (!string.IsNullOrEmpty(levelStr))
                {
                    int.TryParse(levelStr, out var parsedLevel);
                    level = parsedLevel;
                }

                // 获取报警历史
                var alarmHistory = systemManager.AlarmMgr.GetAlarmHistory(startDate, endDate);
                
                // 如果指定了级别，进行过滤
                if (level.HasValue)
                {
                    // 这里可以添加级别的过滤逻辑
                    // 由于AlarmLevel是枚举，需要转换一下
                    try
                    {
                        var filterLevel = (OmniFrame.Core.AlarmLevel)level.Value;
                        alarmHistory = alarmHistory.Where(a => a.Level == filterLevel).ToList();
                    }
                    catch (Exception ex) { Logger.Warning($"报警级别过滤转换失败: {levelStr}", ex); }
                }

                return alarmHistory;
            }
            catch (Exception ex)
            {
                Logger.Error("获取报警历史失败", ex);
                return new { error = "Failed to get alarm history" };
            }
        }

        /// <summary>
        /// 获取产量历史
        /// </summary>
        /// <param name="request">HTTP请求</param>
        /// <returns>产量历史数据</returns>
        private async Task<object> GetProductionHistoryAsync(HttpListenerRequest request)
        {
            try
            {
                var systemManager = _systemManager;
                if (systemManager == null || systemManager.ProductManager == null)
                {
                    return new { error = "Production management system not available" };
                }

                // 解析查询参数
                string startDateStr = request.QueryString["startDate"];
                string endDateStr = request.QueryString["endDate"];
                string productId = request.QueryString["productId"];

                DateTime? startDate = null;
                DateTime? endDate = null;

                if (!string.IsNullOrEmpty(startDateStr))
                {
                    DateTime.TryParse(startDateStr, out var parsedStartDate);
                    startDate = parsedStartDate;
                }

                if (!string.IsNullOrEmpty(endDateStr))
                {
                    DateTime.TryParse(endDateStr, out var parsedEndDate);
                    endDate = parsedEndDate;
                }

                // 获取产量历史
                var productionHistory = systemManager.ProductManager.GetProductionHistory(startDate, endDate, productId);
                return productionHistory;
            }
            catch (Exception ex)
            {
                Logger.Error("获取产量历史失败", ex);
                return new { error = "Failed to get production history" };
            }
        }

        /// <summary>
        /// 获取系统健康状态
        /// </summary>
        /// <returns>系统健康状态</returns>
        private object GetSystemHealthAsync()
        {
            try
            {
                var systemManager = _systemManager;
                if (systemManager == null)
                {
                    return new { error = "System manager not available" };
                }

                var healthStatus = new
                {
                    timestamp = DateTime.Now,
                    subsystems = new
                    {
                        plc = new
                        {
                            status = systemManager.PlcManager?.IsConnected ?? false ? "online" : "offline",
                            lastUpdated = DateTime.Now
                        },
                        motion = new
                        {
                            status = systemManager.MotionManager?.IsConnected ?? false ? "online" : "offline",
                            lastUpdated = DateTime.Now
                        },
                        io = new
                        {
                            status = systemManager.IoManager?.IsConnected ?? false ? "online" : "offline",
                            lastUpdated = DateTime.Now
                        },
                        robot = new
                        {
                            status = "offline",
                            lastUpdated = DateTime.Now
                        },
                        alarm = new
                        {
                            status = "online",
                            activeAlarms = systemManager.AlarmMgr?.GetActiveAlarms()?.Count ?? 0,
                            lastUpdated = DateTime.Now
                        },
                        product = new
                        {
                            status = "online",
                            lastUpdated = DateTime.Now
                        },
                        user = new
                        {
                            status = "online",
                            lastUpdated = DateTime.Now
                        }
                    },
                    system = new
                    {
                        uptime = DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime,
                        memoryUsage = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024,
                        cpuUsage = GetCpuUsage(),
                        lastUpdated = DateTime.Now
                    }
                };

                return healthStatus;
            }
            catch (Exception ex)
            {
                Logger.Error("获取系统健康状态失败", ex);
                return new { error = "Failed to get system health status" };
            }
        }

        /// <summary>
        /// 获取CPU使用率
        /// </summary>
        /// <returns>CPU使用率</returns>
        private float GetCpuUsage()
        {
            try
            {
                var cpuCounter = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue();
                System.Threading.Thread.Sleep(100);
                return cpuCounter.NextValue();
            }
            catch (Exception ex)
            {
                Logger.Warning($"获取CPU使用率失败: {ex.Message}", ex);
                return 0;
            }
        }
    }

    /// <summary>
    /// API响应基类
        /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ApiResponse()
        {
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// 创建成功响应
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="message">消息</param>
        /// <returns>成功响应</returns>
        public static ApiResponse CreateSuccess(object data = null, string message = "操作成功")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// 创建失败响应
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="data">数据</param>
        /// <returns>失败响应</returns>
        public static ApiResponse CreateError(string message, object data = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Data = data
            };
        }
    }
}
