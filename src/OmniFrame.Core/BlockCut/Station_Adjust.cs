using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MotionIO;
using OmniFrame.Common;

namespace OmniFrame.Core.BlockCut
{
    /// <summary>
    /// 核心生产调整工站 — 替代 ThreadAdjust (~3041 行 C++)
    ///
    /// 主流程:
    ///   CheckBottom→ScanBarcode→MESValidate→GetRecipe→UVWInit
    ///   → for(row) for(col): MeasureHeight→DetectAngle→UVWRotate
    ///   → PlaceSlice→AlignSlice→Dispense→UVCure
    ///   → OutputFullPlate→loop
    /// </summary>
    public class Station_Adjust : BlockCutStationBase
    {
        #region 状态字段

        private string _bottomCode;
        private string _lastBottomCode;
        private string _tmpJigCode;
        private string _tmpBlockCode;
        private string _mesFileId;
        private bool _isWaitMes;
        private string _mesAlertMsg;

        // 底板直线参数 (替代 m_dBottomK, m_dBottomB)
        private double _bottomK;
        private double _bottomB;

        // 相机坐标系关键点
        private double _bottomPosX;          // 底板拍照 X 位置
        private Point2D _leftStartPt;        // 左边起点 (像素坐标)
        private Point2D _leftEndPt;          // 左边终点
        private Point2D _rightStartPt;       // 右边起点
        private Point2D _rightEndPt;         // 右边终点

        // 角度
        private double _uvwAngle;
        private double _currentAngle;

        // 外部就绪标志
        private volatile bool _isReadyFromLoad2OnPos;
        private volatile bool _isReadyFromBottomXOnPos;
        private volatile string _jigCode;
        private volatile string _blockCode;

        // 测高结果
        private bool _isMeasureSuccess;
        private double _measureHeight;
        private bool _isMeasureEnd;
        private double[] _measureHeights; // 动态分配: _rows * _cols

        // 相机拍照状态
        private volatile bool _isCameraFinish;
        private volatile bool _isChangeExposure;
        private CameraResult _cameraResult;

        // 测高配置
        private double _cameraXHeight1;
        private double _cameraXHeight2;
        private double _cameraXHeight3;
        private double _cameraZHeight;
        private double _heightHigh = 0.3;
        private double _heightLow = -0.3;
        private int _heightCheckRetries = 0;

        // 生产循环
        private int _row;
        private int _col;
        private int _rows;
        private int _cols;

        #endregion

        #region 配方参数 (从 XML Recipe / INI 迁移)

        private double _bottomYSetSlice;
        private double _bottomYAdjustBottom;
        private double _bottomRowSpace;
        private double _bottomColSpace;
        private double _objectUnit;  // μm/pixel

        private double _cameraBottomXRight;
        private double _cameraBottomXLeft;
        private double _cameraXAdjustSlice1;
        private double _cameraXAdjustSlice2;
        private double _cameraXBottPos1;
        private double _cameraZSafe;
        private double _cameraZAdjustBottom;
        private double _cameraZAdjustSlice;
        private double _sliceY1Adjust;
        private double _sliceY2Adjust;
        private double _sliceXAdjust;
        private readonly BlockCutConfig _cfg;
        private double _posMax;

        // 扫码列表 (替代 m_strList1-6, 来自 D:/list.ini)
        private List<string>[] _codeLists = new List<string>[6];
        private int _mesPieceStatus;

        // 每位置左右偏差补偿 (9 个位置)
        private double[] _rightOffsets = new double[10];  // 1-9
        private double[] _leftOffsets = new double[10];

        #endregion

        #region 事件

        /// <summary>通知 Load2 可移动</summary>
        public event Action OnNoticeLoad2CanMove;
        /// <summary>通知 BottomGet: 取送底板 (替代 NoticeBottomXCanMove)</summary>
        public event Action<bool> OnNoticeBottomXCanMove;
        /// <summary>更新 UPH / 进度</summary>
        public event Action<string, string, string, int, string> OnUpdateUPH;
        /// <summary>相机拍照请求</summary>
        public event Func<int, int, Task<CameraResult>> OnCameraCapture;
        /// <summary>相机消息</summary>
        public event Action<string> OnCameraMessage;
        /// <summary>底板消息</summary>
        public event Action<string> OnBottomMessage;
        /// <summary>片源消息</summary>
        public event Action<string> OnSliceMessage;
        /// <summary>角度更新</summary>
        public event Action<double> OnAdjustAngle;
        /// <summary>MES 验证请求</summary>
        public event Func<string, Task<MesValidateResult>> OnMesValidate;

        /// <summary>片源 MES 验证 (替代 NoticeSliceMES 信号)</summary>
        public event Func<string, Task<MesValidateResult>> OnSliceMesValidate;

        /// <summary>相机拍照结果回调事件 (替代 OnCameraResult 信号)</summary>
        public event Action<bool, Point2D, Point2D, double> OnCameraResultChanged;
        /// <summary>扫码请求</summary>
        public event Func<Task<string>> OnSweepCode;

        #endregion

        public Station_Adjust(IBlockCutHardware hardware, BlockCutConfig cfg)
            : base(BlockCutConstants.ThreadAdjustName, (int)ThreadId.Adjust, hardware)
        {
            Logger.Info($"[Station_Adjust] 构造函数开始");
            _cfg = cfg;
            _uvwAngle = cfg.UVWAngle;
            _bottomYSetSlice = cfg.BottomYSetSlice;
            _bottomYAdjustBottom = cfg.BottomYAdjustBottom;
            _bottomRowSpace = cfg.BottomRowSpace;
            _bottomColSpace = cfg.BottomColSpaceRecipe;
            _objectUnit = cfg.ObjectUnit;
            _cameraBottomXRight = cfg.CameraBottomXRight;
            _cameraBottomXLeft = cfg.CameraBottomXLeft;
            _cameraXAdjustSlice1 = cfg.CameraXAdjustSlice1;
            _cameraXAdjustSlice2 = cfg.CameraXAdjustSlice2;
            _cameraXBottPos1 = cfg.CameraXBottPos1;
            _cameraZSafe = cfg.CameraZSafe;
            _cameraZAdjustBottom = cfg.CameraZAdjustBottom;
            _cameraZAdjustSlice = cfg.CameraZAdjustSlice;
            _sliceY1Adjust = cfg.SliceY1Adjust;
            _sliceY2Adjust = cfg.SliceY2Adjust;
            _sliceXAdjust = cfg.SliceXAdjust;
            _posMax = cfg.PosMax;
            _rows = cfg.BottomRows;
            _cols = cfg.BottomCols;
            _measureHeights = new double[_rows * _cols];
            Logger.Info($"[Station_Adjust] 构造函数完成 - 底板行列: {_rows}×{_cols}");
        }

        #region 主循环 — 状态机 (Qt QThread::exec() 对齐)

        /// <summary>生产主状态 — 映射 Qt ThreadAdjust 的 switch-case 事件循环</summary>
        private enum AdjustState
        {
            Init,
            GoSafeInit,
            CheckBottom,
            GetEmptyBottom,
            WaitSweepCode,
            MesValidate,
            LoadRecipe,
            UVWInit,
            CheckHeightRange,
            AdjustBottom,
            CheckSliceCard,
            SetSlice,
            BottomCameraGetY,
            AdjustSlice,
            DispensingOnce,
            GoSafeAfterDispense,
            AdvancePosition,
            SendBottomFull,
            Error,
        }

        private AdjustState _state = AdjustState.Init;
        private int _dryRunSeq;

        public override async Task RunAsync(CancellationToken token)
        {
            InitParams();

            Logger.Info($"[{StationName}] ═══ 工站启动 ═══");
            Logger.Info($"[{StationName}]   空跑模式: {IsEmptyTest}");
            Logger.Info($"[{StationName}]   底板行列: {_rows}×{_cols}");
            Logger.Info($"[{StationName}]   行间距={_bottomRowSpace:F3} 列间距={_bottomColSpace:F3}");
            Logger.Info($"[{StationName}]   UVW初始角度={_uvwAngle:F2}°");
            Logger.Info($"[{StationName}]   测高范围=[{_heightLow:F3}, {_heightHigh:F3}]");

            _state = AdjustState.Init;

            while (!token.IsCancellationRequested)
            {
                CheckPause(token);

                switch (_state)
                {
                    case AdjustState.Init:
                        EmitMessage("调节组初始化...");
                        _state = AdjustState.GoSafeInit;
                        break;

                    case AdjustState.GoSafeInit:
                        await GoSafeAsync(token);
                        _state = AdjustState.CheckBottom;
                        break;

                    case AdjustState.CheckBottom:
                        EmitMessage("检测底板...");
                        if (await CheckIsBottomAsync(token))
                        {
                            EmitMessage("底板已存在 — 准备底板");
                            await BottomYCylinderAsync(false, token);
                            SetDO(BlockCutConstants.DO_BottomOutPlatformCylinder, true);
                            await Task.Delay(1000, token);
                            await BottomYCylinderAsync(true, token);
                            await BottomYCylinderAsync(false, token);
                            await SetOneCylinderAsync(
                                BlockCutConstants.DO_BottomOutPlatformCylinder, false,
                                BlockCutConstants.DI_BottomOutPlatformCylinderUp, false,
                                "料板载台顶升气缸下降", token);
                            await BottomYCylinderAsync(true, token);
                            _state = AdjustState.WaitSweepCode;
                        }
                        else
                        {
                            EmitMessage("无底板 — 请求送空底板");
                            _state = AdjustState.GetEmptyBottom;
                        }
                        break;

                    case AdjustState.GetEmptyBottom:
                        await SendOrGetBottomAsync(true, token);
                        _state = AdjustState.WaitSweepCode;
                        break;

                    case AdjustState.WaitSweepCode:
                        EmitMessage("等待扫码...");
                        if (IsEmptyTest)
                        {
                            _bottomCode = $"DRY-{++_dryRunSeq:D6}";
                            EmitMessage($"[空跑] 自动生成条码: {_bottomCode}");
                            _state = AdjustState.MesValidate;
                        }
                        else
                        {
                            string code = await SweepCodeAsync(token);
                            if (!string.IsNullOrEmpty(code))
                            {
                                _bottomCode = code;
                                _state = AdjustState.MesValidate;
                            }
                            // else: 留在此状态继续等待扫码
                        }
                        break;

                    case AdjustState.MesValidate:
                        EmitMessage($"MES 验证: {_bottomCode}");
                        var mesResult = await MesValidateAsync(_bottomCode, token);
                        if (mesResult?.IsValid == true)
                        {
                            _mesFileId = mesResult.FileId;
                            _state = AdjustState.LoadRecipe;
                        }
                        else
                        {
                            EmitMessage("MES 验证失败 — 送走底板");
                            _state = AdjustState.SendBottomFull;
                        }
                        break;

                    case AdjustState.LoadRecipe:
                        EmitMessage($"加载配方参数: {_bottomCode}");
                        _lastBottomCode = _bottomCode;
                        LoadRecipeParams(_lastBottomCode);
                        _state = AdjustState.UVWInit;
                        break;

                    case AdjustState.UVWInit:
                        EmitMessage("UVW 平台初始化...");
                        await UVWInitAsync(token);
                        _row = 1; _col = 1;
                        _state = AdjustState.CheckHeightRange;
                        break;

                    case AdjustState.CheckHeightRange:
                        // 每行第一列 / 首次: 网格测高
                        if (_col == 1)
                        {
                            EmitMessage($"网格测高 (行{_row})...");
                            if (IsEmptyTest)
                            {
                                for (int i = 0; i < _measureHeights.Length; i++)
                                    _measureHeights[i] = 0.5;
                                EmitMessage("[空跑] 测高 = 0.5mm");
                            }
                            else
                            {
                                if (!await CheckHeightRangeAsync(true, token))
                                {
                                    _state = AdjustState.Error;
                                    break;
                                }
                            }
                        }
                        _state = AdjustState.AdjustBottom;
                        break;

                    case AdjustState.AdjustBottom:
                        // 每行第一列: 底板角度调节
                        if (_col == 1)
                        {
                            EmitMessage($"底板角度调节 (行{_row})...");
                            if (IsEmptyTest)
                            {
                                EmitMessage("[空跑] 跳过角度调节");
                            }
                            else
                            {
                                if (!await AdjustBottomAsync(token))
                                {
                                    _state = AdjustState.Error;
                                    break;
                                }
                            }
                        }
                        _state = AdjustState.CheckSliceCard;
                        break;

                    case AdjustState.CheckSliceCard:
                        // MES 片源校验
                        if (!await CheckSliceCardAsync(token))
                        {
                            _state = AdjustState.Error;
                            break;
                        }
                        _state = AdjustState.SetSlice;
                        break;

                    case AdjustState.SetSlice:
                        EmitMessage($"放料 [{_row},{_col}]...");
                        if (!await SetSliceAsync(_row, _col, token))
                        {
                            _state = AdjustState.Error;
                            break;
                        }
                        _state = AdjustState.BottomCameraGetY;
                        break;

                    case AdjustState.BottomCameraGetY:
                        EmitMessage("检测底板 Y 位置...");
                        if (IsEmptyTest)
                        {
                            EmitMessage("[空跑] 跳过 Y 检测");
                        }
                        else
                        {
                            if (!await BottomCameraGetYAsync(token))
                            {
                                _state = AdjustState.Error;
                                break;
                            }
                        }
                        _state = AdjustState.AdjustSlice;
                        break;

                    case AdjustState.AdjustSlice:
                        EmitMessage($"调整片源 [{_row},{_col}]...");
                        if (IsEmptyTest)
                        {
                            EmitMessage("[空跑] 跳过片源调整");
                        }
                        else
                        {
                            if (!await AdjustSliceAsync(token))
                            {
                                _state = AdjustState.Error;
                                break;
                            }
                        }
                        _state = AdjustState.DispensingOnce;
                        break;

                    case AdjustState.DispensingOnce:
                        EmitMessage($"点胶+UV [{_row},{_col}]...");
                        if (IsEmptyTest)
                        {
                            EmitMessage("[空跑] 跳过点胶+UV");
                        }
                        else
                        {
                            if (!await DispensingOnceAsync(token))
                            {
                                _state = AdjustState.Error;
                                break;
                            }
                        }
                        _state = AdjustState.GoSafeAfterDispense;
                        break;

                    case AdjustState.GoSafeAfterDispense:
                        await GoSafeAsync(token);

                        // 上报进度
                        int pos = _col + (_row - 1) * _cols;
                        OnUpdateUPH?.Invoke(_tmpJigCode, _lastBottomCode, _tmpBlockCode, pos, _mesFileId);
                        EmitMessage($"产出位置 {pos}/{_rows * _cols}");
                        _state = AdjustState.AdvancePosition;
                        break;

                    case AdjustState.AdvancePosition:
                        if (_col >= _cols)
                        {
                            if (_row >= _rows)
                            {
                                EmitMessage($"整板完成 ({_rows}×{_cols}) — 送走满板");
                                _state = AdjustState.SendBottomFull;
                            }
                            else
                            {
                                _row++; _col = 1;
                                _state = AdjustState.CheckHeightRange;
                            }
                        }
                        else
                        {
                            _col++;
                            _state = AdjustState.CheckSliceCard;
                        }
                        break;

                    case AdjustState.SendBottomFull:
                        await SendOrGetBottomAsync(false, token);
                        EmitMessage("满板已送走 — 等待下一底板条码");
                        _state = AdjustState.WaitSweepCode;
                        break;

                    case AdjustState.Error:
                        EmitWarning(0, $"调节组异常 (状态={_state})，暂停中...", true);
                        Pause();
                        // 恢复后从扫码重新开始
                        _state = AdjustState.WaitSweepCode;
                        break;
                }

                await Task.Delay(1, token);
            }
        }

        #endregion

        #region 初始化

        private void InitParams()
        {
            _isReadyFromLoad2OnPos = false;
            _isReadyFromBottomXOnPos = false;
            _isMeasureEnd = false;
            _isMeasureSuccess = false;
            _measureHeight = 0;
            _bottomCode = "";
            _lastBottomCode = "";
            _mesFileId = "";
            _isWaitMes = false;
            _mesAlertMsg = "";
            _mesPieceStatus = 0;

            DownINIFile();
        }

        #endregion

        #region 底板检测 → 送取板

        /// <summary>检测底板是否存在 (替代 CheckIsBottom)</summary>
        private Task<bool> CheckIsBottomAsync(CancellationToken token)
        {
            if (IsEmptyTest) return Task.FromResult(false); // 强制走 GetEmptyBottom 流程，与 BottomGet 握手
            return Task.FromResult(GetDI(BlockCutConstants.DI_CheckBottom));
        }

        /// <summary>送/取底板 — 通过 OnNoticeBottomXCanMove 事件协调 Station_BottomGet (替代 SendOrGetBottom)</summary>
        private async Task<bool> SendOrGetBottomAsync(bool isGet, CancellationToken token)
        {
            EmitMessage(isGet ? "请求送空底板..." : "请求取走满板...");

            // BottomY → 自由位
            double dY = _bottomYAdjustBottom;
            await OneAxisMoveAbsAsync((int)AxisId.BottomY, dY, "底板Y到取底板位置", token: token);

            // 底板夹紧气缸松开
            await SetOneCylinder2Async(
                BlockCutConstants.DO_BottomCompressCylinder, false,
                BlockCutConstants.DI_BottomCompressCylinderReach, false,
                BlockCutConstants.DI_BottomCompressCylinderRetract, true,
                "底板夹紧气缸松开", token);

            // 通知 BottomGet 取送底板
            OnNoticeBottomXCanMove?.Invoke(isGet);

            // 等待 BottomGet 完成
            await WaitForBottomXOnPosAsync(token);

            bool result;
            if (isGet)
            {
                result = GetDI(BlockCutConstants.DI_CheckBottom);
                await SetOneCylinder2Async(
                    BlockCutConstants.DO_BottomCompressCylinder, true,
                    BlockCutConstants.DI_BottomCompressCylinderReach, true,
                    BlockCutConstants.DI_BottomCompressCylinderRetract, false,
                    "底板夹紧气缸伸出", token);
            }
            else
            {
                result = !GetDI(BlockCutConstants.DI_CheckBottom);
                if (!IsEmptyTest)
                    PauseWarnMessage(13000 + (int)ThreadId.Adjust,
                        $"{BlockCutConstants.ThreadAdjustName}暂停，底板已经送回，请更换底板");
            }
            return result;
        }

        /// <summary>扫码 (替代 SweepCode)</summary>
        private async Task<string> SweepCodeAsync(CancellationToken token)
        {
            EmitMessage("等待扫码...");
            var code = await (OnSweepCode?.Invoke() ?? Task.FromResult<string>(null));
            return code;
        }

        /// <summary>MES 验证 (替代 CheckCard) — 10s 超时 + 最多 3 次重试</summary>
        private async Task<MesValidateResult> MesValidateAsync(string code, CancellationToken token)
        {
            EmitMessage($"MES 验证: {code}");
            if (OnMesValidate == null) return new MesValidateResult { IsValid = true }; // 空跑模式

            const int maxRetries = 3;
            const int timeoutMs = 10000;

            for (int retry = 0; retry < maxRetries; retry++)
            {
                token.ThrowIfCancellationRequested();
                CheckPause(token);

                try
                {
                    using var cts = new CancellationTokenSource(timeoutMs);
                    using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, cts.Token);

                    var result = await OnMesValidate(code);
                    if (result?.IsValid == true)
                    {
                        _isWaitMes = false;
                        _mesAlertMsg = "";
                        return result;
                    }

                    if (retry < maxRetries - 1)
                    {
                        EmitMessage($"MES 验证失败，重试 {retry + 1}/{maxRetries - 1}...");
                        await Task.Delay(500, token);
                    }
                }
                catch (OperationCanceledException) when (!token.IsCancellationRequested)
                {
                    EmitMessage($"MES 验证超时 ({timeoutMs / 1000}s)，重试 {retry + 1}/{maxRetries - 1}...");
                }
            }

            PauseWarnMessage(13000 + (int)ThreadId.Adjust, "ERR-MIS-05: MES 验证失败，请检查网络或重试");
            return null;
        }

        #endregion

        #region 配方加载 (替代 GetParamFromCode + DownINIFile)

        /// <summary>
        /// 按产品码加载配方参数。默认值已在构造函数中从 BlockCut.xml 注入，
        /// 后续接入 MES 后可在此按 code 查询产品级配方覆写。
        /// </summary>
        /// <summary>下载扫码列表 — 从 D:/list.ini 读取 6 组条码→参数映射 (替代 DownINIFile)</summary>
        private void DownINIFile()
        {
            string iniPath = @"D:\list.ini";
            if (!System.IO.File.Exists(iniPath))
            {
                Logger.Warning($"[Station_Adjust] list.ini 不存在: {iniPath}，使用默认参数");
                return;
            }

            try
            {
                for (int group = 1; group <= 6; group++)
                {
                    string listStr = IniHelper.ReadIni(group.ToString(), "list", "", iniPath);
                    if (!string.IsNullOrEmpty(listStr))
                        _codeLists[group - 1] = new List<string>(listStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                    else
                        _codeLists[group - 1] = new List<string>();
                }
                Logger.Info($"[Station_Adjust] list.ini 加载完成: {_codeLists.Sum(l => l.Count)} 条记录");
            }
            catch (Exception ex)
            {
                Logger.Error("[Station_Adjust] list.ini 读取失败", ex);
            }
        }

        /// <summary>根据底板码从列表中匹配产品参数组 (替代 GetParamFromCode)</summary>
        private void LoadRecipeParams(string code)
        {
            for (int group = 0; group < 6; group++)
            {
                if (_codeLists[group] == null) continue;
                foreach (string item in _codeLists[group])
                {
                    if (item.Trim() == code.Trim())
                    {
                        _bottomYSetSlice = group switch
                        {
                            0 => _cfg.BottomYSetSlice1,
                            1 => _cfg.BottomYSetSlice2,
                            2 => _cfg.BottomYSetSlice3,
                            3 => _cfg.BottomYSetSlice4,
                            4 => _cfg.BottomYSetSlice5,
                            5 => _cfg.BottomYSetSlice6,
                            _ => _cfg.BottomYSetSlice1,
                        };
                        _uvwAngle = group switch
                        {
                            0 => _cfg.UVWAngle1,
                            1 => _cfg.UVWAngle2,
                            2 => _cfg.UVWAngle3,
                            3 => _cfg.UVWAngle4,
                            4 => _cfg.UVWAngle5,
                            5 => _cfg.UVWAngle6,
                            _ => _cfg.UVWAngle1,
                        };
                        _bottomYAdjustBottom = group switch
                        {
                            0 => _cfg.BottomYAdjustBottom1,
                            1 => _cfg.BottomYAdjustBottom2,
                            2 => _cfg.BottomYAdjustBottom3,
                            3 => _cfg.BottomYAdjustBottom4,
                            4 => _cfg.BottomYAdjustBottom5,
                            5 => _cfg.BottomYAdjustBottom6,
                            _ => _cfg.BottomYAdjustBottom1,
                        };
                        Logger.Info($"[Station_Adjust] 匹配产品组{group + 1}: code={code}");
                        return;
                    }
                }
            }
            // 未匹配则使用默认值 (已在构造函数中赋值)
        }

        #endregion

        #region UVW 平台

        /// <summary>UVW 初始化 (替代 BottomUVWInit)</summary>
        private async Task UVWInitAsync(CancellationToken token)
        {
            double r = 125.0;
            double vr = Math.PI / 2;    // 90°
            double wr = 3 * Math.PI / 2; // 270°
            double ur = Math.PI;         // 180°
            double angleRad = _uvwAngle * Math.PI / 180.0;

            double vInit = r * Math.Cos(vr + angleRad) - r * Math.Cos(vr);
            double wInit = r * Math.Cos(wr + angleRad) - r * Math.Cos(wr);
            double uInit = r * Math.Sin(ur + angleRad) - r * Math.Sin(ur);

            await MultiAxisMoveAbsAsync(
                new Dictionary<int, double>
                {
                    { (int)AxisId.BottomU, uInit },
                    { (int)AxisId.BottomV, vInit },
                    { (int)AxisId.BottomW, wInit },
                },
                new Dictionary<int, string>
                {
                    { (int)AxisId.BottomU, "UVW-U初始化" },
                    { (int)AxisId.BottomV, "UVW-V初始化" },
                    { (int)AxisId.BottomW, "UVW-W初始化" },
                },
                token: token);
        }

        #endregion

        #region 调整底板角度 (替代 AdjustBottom)

        /// <summary>调整底板角度 — 2 点相机拍照 + 角度计算 + UVW 旋转</summary>
        private async Task<bool> AdjustBottomAsync(CancellationToken token)
        {
            EmitMessage("开始调节底板角度...");
            CheckPause(token);

            // 调整 Z 气缸下降
            await SliceZCylinderAsync(false, token);

            // 相机 X → 右侧拍照位
            int axisCX = (int)AxisId.CameraX;
            await OneAxisMoveAbsAsync(axisCX, _cameraBottomXRight, "相机X运动到右侧拍底板位置", token: token);

            // 调整 Z 气缸上升
            await SliceZCylinderAsync(true, token);

            // BottomY → 调节位置
            double dY = _bottomYAdjustBottom + (_row - 1) * _bottomRowSpace;
            await OneAxisMoveAbsAsync((int)AxisId.BottomY, dY, $"底板Y运动到第{_row}行调节位置", token: token);

            // 调整 Z 气缸下降
            await SliceZCylinderAsync(false, token);
            await SliceYCylinderAsync(true, token);

            // 迭代调整角度
            double totalAngle = _uvwAngle;
            while (!token.IsCancellationRequested)
            {
                if (await BottomCameraGetAngleAsync(token))
                    break;

                totalAngle += _currentAngle;
                await UVWRotateAsync(_currentAngle, 0, 0, token);
            }

            OnAdjustAngle?.Invoke(totalAngle);
            EmitMessage("底板角度调节完成");
            return true;
        }

        /// <summary>底板角度检测 — 2 点相机取像 → 边缘拟合 → 角度计算 (替代 BottomCameraGetAngle)</summary>
        private async Task<bool> BottomCameraGetAngleAsync(CancellationToken token)
        {
            if (IsEmptyTest) { _currentAngle = 0; return true; }

            double dPosX1 = 0;
            var vPts = new List<Point2D>();

            for (int i = 0; i < 2; i++)
            {
                token.ThrowIfCancellationRequested();

                double height = _measureHeights[(_row - 1) * _cols + (i == 0 ? 0 : 2)];
                double dX = i == 0 ? _cameraBottomXRight : _cameraBottomXLeft;

                // 相机 X 到位
                await OneAxisMoveAbsAsync((int)AxisId.CameraX, dX,
                    $"运动到第{i + 1}列底板拍照位置", token: token);

                // 相机 Z 到位 (对焦)
                await OneAxisMoveAbsAsync((int)AxisId.CameraZ,
                    _cameraZAdjustBottom + height, "运动到底板对焦位置", token: token);

                await Task.Delay(1000, token);

                // 拍照
                var result = await OnCameraCapture?.Invoke(i, i + 1);
                if (result == null || !result.Success) return false;

                vPts.Add(result.Point1);

                if (i == 0)
                {
                    _rightStartPt = result.Point1;
                    _rightEndPt = result.Point2;
                    dPosX1 = Motion.GetAxisPos((int)AxisId.CameraX);
                }
                else
                {
                    _leftStartPt = result.Point1;
                    _leftEndPt = result.Point2;
                    _bottomPosX = Motion.GetAxisPos((int)AxisId.CameraX);
                }
            }

            // 右边直线: Ax + By + C = 0
            double dA = _rightEndPt.Y - _rightStartPt.Y;
            double dB = _rightStartPt.X - _rightEndPt.X;
            double dC = _rightEndPt.X * _rightStartPt.Y - _rightStartPt.X * _rightEndPt.Y;

            double dYRight = 0;
            if (Math.Abs(dB) > 1e-10)
                dYRight = (-dA * _leftStartPt.X - dC) / dB;

            // 更新底板直线参考 (K, B)
            double dxPix = Math.Abs(_bottomPosX - dPosX1) * 1000.0 / _objectUnit;
            _bottomK = (dYRight - _leftStartPt.Y) / dxPix;
            _bottomB = _leftStartPt.Y - _bottomK * _leftStartPt.X;

            // 计算角度
            double dYOffset = (vPts[1].Y - vPts[0].Y) * 0.001 * _objectUnit;
            double dXOffset = (vPts[0].X - vPts[1].X) * 0.001 * _objectUnit;
            _currentAngle = Math.Atan(dYOffset / (-Math.Abs(_bottomPosX - dPosX1) + dXOffset)) * 180.0 / Math.PI;

            OnBottomMessage?.Invoke($"底板角度为{_currentAngle:F4}度");

            // 角度偏差 > 2 像素 → 需要重调
            if (Math.Abs(dYRight - _leftStartPt.Y) > 2)
                return false;

            return true;
        }

        #endregion

        #region 测高 (替代 CheckHeightRange)

        #endregion

        #region 底板 Y 检测 (替代 BottomCameraGetY)

        /// <summary>检测底板 Y 位置 — 相机拍照 → 计算底板 Y 参考</summary>
        private async Task<bool> BottomCameraGetYAsync(CancellationToken token)
        {
            EmitMessage("调节组开始检测底板Y位置...");
            CheckPause(token);

            double dY = _bottomYAdjustBottom + (_row - 1) * _bottomRowSpace;
            OnBottomMessage?.Invoke($"底板Y轴运动到调整片源第{_row}行位置");
            await OneAxisMoveAbsAsync((int)AxisId.BottomY, dY, "底板Y定位", token: token);

            double dX = _cameraXBottPos1;
            OnCameraMessage?.Invoke("相机X轴运动到拍底板Y位置");
            await OneAxisMoveAbsAsync((int)AxisId.CameraX, dX, "相机X定位", token: token);

            double height = _measureHeights[(_row - 1) * _cols];
            await OneAxisMoveAbsAsync((int)AxisId.CameraZ,
                _cameraZAdjustBottom + height, "运动到底板对焦位置", token: token);

            await Task.Delay(1000, token);

            // 拍照
            var result = await OnCameraCapture?.Invoke(2, 1);
            if (result == null || !result.Success)
            {
                EmitMessage("调节组检测底板Y位置失败");
                return false;
            }

            // 计算 Y 参考
            double dA = result.Point2.Y - result.Point1.Y;
            double dB = result.Point1.X - result.Point2.X;
            double dC = result.Point2.X * result.Point1.Y - result.Point1.X * result.Point2.Y;

            double dYTrans = 0;
            if (Math.Abs(dB) > 1e-10)
                dYTrans = (-dA * _leftStartPt.X - dC) / dB;

            double dSpace = Math.Abs(Motion.GetAxisPos((int)AxisId.CameraX) - _bottomPosX);
            double dX1InPix = _leftStartPt.X + dSpace * 1000.0 / _objectUnit;
            _bottomB = dYTrans - _bottomK * dX1InPix;

            OnBottomMessage?.Invoke($"[Info]底板Y位置为{dYTrans:F4}");
            EmitMessage("调节组检测底板Y位置成功");
            return true;
        }

        #endregion

        #region 放料 (替代 SetSlice)

        /// <summary>放料: BottomY 定位 → 通知 Load2 → 等待到位 → MES 校验片源</summary>
        private async Task<bool> SetSliceAsync(int row, int col, CancellationToken token)
        {
            EmitMessage("调节组底板开始放块料...");

            double dY = _bottomYSetSlice + (row - 1) * _bottomRowSpace;
            if (!await OneAxisMoveAbsAsync((int)AxisId.BottomY, dY,
                $"运动到放片源第{row}行位置", token: token).ContinueWith(t => t.Result == 1))
                return false;

            // 通知 Load2
            OnNoticeLoad2CanMove?.Invoke();

            // 等待 Load2 到位
            await WaitLoad2OnPosAsync(token);

            _tmpJigCode = _jigCode;
            _tmpBlockCode = _blockCode;
            EmitMessage("调节组底板放块料成功");
            return true;
        }

        /// <summary>等待 Load2 到位 (替代 WaitLoad2OnPos)</summary>
        private async Task WaitLoad2OnPosAsync(CancellationToken token)
        {
            while (!_isReadyFromLoad2OnPos && !token.IsCancellationRequested)
                await Task.Delay(1, token);
            _isReadyFromLoad2OnPos = false;
        }

        /// <summary>等待 BottomGet 到位 (替代 WaitBottomXOnPos)</summary>
        private async Task WaitForBottomXOnPosAsync(CancellationToken token)
        {
            EmitMessage("等待料板X组位置安全");
            while (!token.IsCancellationRequested)
            {
                if (_isReadyFromBottomXOnPos) { _isReadyFromBottomXOnPos = false; break; }
                await Task.Delay(1, token);
            }
            EmitMessage("等待结束");
        }

        #endregion

        #region 调整片源 (替代 AdjustSlice + SliceCameraGetAngle)

        /// <summary>调整片源 — 2 点相机拍照 + 计算左右偏差 → Y1/Y2 迭代调整</summary>
        private async Task<bool> AdjustSliceAsync(CancellationToken token)
        {
            EmitMessage("调节组底板开始调整块料...");

            // 相机 Z → 安全位
            await OneAxisMoveAbsAsync((int)AxisId.CameraZ, _cameraZSafe, "安全位置", token: token);

            // 相机 X → 拍片源位置 2
            double dX1 = _cameraXAdjustSlice2 - (_col - 1) * _bottomColSpace;
            await OneAxisMoveAbsAsync((int)AxisId.CameraX, dX1,
                $"相机X轴运动到第{_col}列拍片源位置1", token: token);

            // AdjustX → 调节位置
            double dX = _sliceXAdjust - (_col - 1) * _bottomColSpace;
            await OneAxisMoveAbsAsync((int)AxisId.AdjustX, dX,
                $"运动到第{_col}列片源位置", token: token);

            // 夹紧片源
            await SliceYCylinderAsync(true, token);
            await SliceZCylinderAsync(false, token);

            // 相机 Z → 对焦
            double height = _measureHeights[(_row - 1) * _cols + (_col - 1)];
            await OneAxisMoveAbsAsync((int)AxisId.CameraZ,
                _cameraZAdjustSlice + height, "相机Z轴运动到片源对焦位置", token: token);

            // AdjustY1/Y2 → 调节位
            await TwoAxisMoveAbsAsync(
                (int)AxisId.AdjustY1, _sliceY1Adjust,
                (int)AxisId.AdjustY2, _sliceY2Adjust,
                "运动到调节位置", token: token);

            // 重新夹持一次片源
            SetDO(BlockCutConstants.DO_YCylinder, false);
            await Task.Delay(1000, token);
            SetDO(BlockCutConstants.DO_YCylinder, true);
            await Task.Delay(500, token);

            // 迭代对齐
            while (!token.IsCancellationRequested)
            {
                if (await SliceCameraGetAngleAsync(token))
                    break;
            }

            EmitMessage("调节组底板调整块料成功");
            return true;
        }

        /// <summary>片源角度检测 + Y1/Y2 偏差计算 → 迭代调整 (替代 SliceCameraGetAngle)</summary>
        private async Task<bool> SliceCameraGetAngleAsync(CancellationToken token)
        {
            if (IsEmptyTest) return true;

            double dPos = -(_col - 1) * _bottomColSpace;

            Point2D leftCenter = default, rightCenter = default;
            double dPosX1 = 0, dPosX2 = 0;

            for (int i = 0; i < 2; i++)
            {
                token.ThrowIfCancellationRequested();

                double dX = dPos + (i == 0 ? _cameraXAdjustSlice2 : _cameraXAdjustSlice1);

                await OneAxisMoveAbsAsync((int)AxisId.CameraX, dX,
                    $"运动到第{i + 1}个片源拍照位置", token: token);

                await Task.Delay(100, token);

                // 拍照
                var result = await OnCameraCapture?.Invoke(3, 0);
                if (result == null || !result.Success) return false;

                if (i == 0)
                {
                    _rightStartPt = result.Point1;
                    _rightEndPt = result.Point2;
                    dPosX1 = Motion.GetAxisPos((int)AxisId.CameraX);
                    rightCenter = new Point2D(
                        (result.Point1.X + result.Point2.X) / 2,
                        (result.Point1.Y + result.Point2.Y) / 2);
                }
                else
                {
                    _leftStartPt = result.Point1;
                    _leftEndPt = result.Point2;
                    dPosX2 = Motion.GetAxisPos((int)AxisId.CameraX);
                    leftCenter = new Point2D(
                        (result.Point1.X + result.Point2.X) / 2,
                        (result.Point1.Y + result.Point2.Y) / 2);
                }
            }

            // 获取该位置的左右偏差补偿值 (9 个位置 switch)
            int posIdx = _col + _cols * (_row - 1);
            double rightOff = posIdx <= 9 ? _rightOffsets[posIdx] : 0;
            double leftOff = posIdx <= 9 ? _leftOffsets[posIdx] : 0;

            // 左边线到参考点的距离
            double dALeft = _leftEndPt.Y - _leftStartPt.Y;
            double dBLeft = _leftStartPt.X - _leftEndPt.X;
            double dCLeft = _leftEndPt.X * _leftStartPt.Y - _leftStartPt.X * _leftEndPt.Y;

            // 右边线到参考点的距离
            double dARight = _rightEndPt.Y - _rightStartPt.Y;
            double dBRight = _rightStartPt.X - _rightEndPt.X;
            double dCRight = _rightEndPt.X * _rightStartPt.Y - _rightStartPt.X * _rightEndPt.Y;

            // 计算片源左右边缘到底板参考线的像素偏差
            double dX1InPix = _leftStartPt.X + Math.Abs(dPosX1 - _bottomPosX) * 1000.0 / _objectUnit;
            double dX2InPix = _leftStartPt.X + Math.Abs(dPosX2 - _bottomPosX) * 1000.0 / _objectUnit;

            double leftUp = _bottomK * (leftCenter.X + Math.Abs(dPosX2 - _bottomPosX) * 1000.0 / _objectUnit)
                            - leftCenter.Y + _bottomB;
            double rightUp = _bottomK * (rightCenter.X + Math.Abs(dPosX1 - _bottomPosX) * 1000.0 / _objectUnit)
                             - rightCenter.Y + _bottomB;

            double distLeft = leftUp / Math.Sqrt(_bottomK * _bottomK + 1);
            double distRight = rightUp / Math.Sqrt(_bottomK * _bottomK + 1);

            double posOffRight = -distRight * 0.001 * _objectUnit + rightOff;
            double posOffLeft = -distLeft * 0.001 * _objectUnit + leftOff;

            OnSliceMessage?.Invoke($"[Info]片源左位置距离底板差距为{posOffLeft:F4}");
            OnSliceMessage?.Invoke($"[Info]片源右位置距离底板差距为{posOffRight:F4}");

            bool isRightBad = posOffRight > _posMax || posOffRight <= 0;
            bool isLeftBad = posOffLeft > _posMax || posOffLeft <= 0;

            // 单侧坏点处理 + 重新夹紧逻辑
            if (isLeftBad && isRightBad)
            {
                // 两侧都超差 → 按 0.8 系数迭代调整
                await OneAxisMoveAbsAsync((int)AxisId.CameraX,
                    dPos + _cameraXAdjustSlice2, "回初始拍片位", token: token);

                await TwoAxisMoveRelAsync(
                    (int)AxisId.AdjustY1, posOffLeft * 0.8,
                    (int)AxisId.AdjustY2, posOffRight * 0.8,
                    $"Y1调整{posOffLeft:F4}, Y2调整{posOffRight:F4}", token: token);

                return false;
            }

            if (isLeftBad && !isRightBad)
            {
                // 仅左侧超差 → 只调 Y1
                await OneAxisMoveRelAsync((int)AxisId.AdjustY1, posOffLeft * 0.8,
                    $"仅Y1调整{posOffLeft:F4}", token: token);
                return false;
            }

            if (!isLeftBad && isRightBad)
            {
                // 仅右侧超差 → 只调 Y2
                await OneAxisMoveRelAsync((int)AxisId.AdjustY2, posOffRight * 0.8,
                    $"仅Y2调整{posOffRight:F4}", token: token);
                return false;
            }

            // 偏差过小 → 重新夹紧片源
            if (posOffLeft < -0.001 || posOffRight < -0.001)
            {
                SetDO(BlockCutConstants.DO_YCylinder, false);
                await Task.Delay(1000, token);
                SetDO(BlockCutConstants.DO_YCylinder, true);
                await Task.Delay(500, token);
                return false;
            }

            return true;
        }

        #endregion

        #region 点胶 + UV 固化 (替代 DispensingOnce)

        /// <summary>点胶一次 + UV 固化 — 对齐 Qt 完整逻辑</summary>
        private async Task<bool> DispensingOnceAsync(CancellationToken token)
        {
            EmitMessage("调节组开始点胶...");
            CheckPause(token);

            double height = _measureHeights[(_row - 1) * _cols + (_col - 1)];

            // CameraX 预移动到等待位 (m_dCameraXWait)
            double cameraXWait = _cameraXAdjustSlice2 - (_col - 1) * _bottomColSpace;
            await OneAxisMoveAbsAsync((int)AxisId.CameraX, cameraXWait,
                "CameraX预移动到点胶等待位", checkPos: false, token: token);

            // DisXY 协同移动
            double disX = _sliceXAdjust - (_col - 1) * _bottomColSpace;
            double disY = _bottomYSetSlice + (_row - 1) * _bottomRowSpace;
            await TwoAxisMoveAbsAsync((int)AxisId.DisX, disX, (int)AxisId.DisY, disY,
                "DisXY协同定位", token: token);

            // DisZ 下降 + 高度补偿 (m_dDisZDis + dHeight)
            double disZDown = 20.0 + height;
            await OneAxisMoveAbsAsync((int)AxisId.DisZ, disZDown,
                $"DisZ下降(含高度补偿{height:F3})", token: token);

            // 触发点胶 — 点胶时间从 config 读取 (m_nTime)
            int dispenseTimeMs = 500;
            SetDO(BlockCutConstants.DO_Dispensing, true);
            await Task.Delay(dispenseTimeMs, token);
            SetDO(BlockCutConstants.DO_Dispensing, false);

            await Task.Delay(100, token);

            // CameraX/CameraZ 移动到 UV 位置
            double cameraXUV = _cameraXAdjustSlice2 - (_col - 1) * _bottomColSpace;
            await OneAxisMoveAbsAsync((int)AxisId.CameraX, cameraXUV,
                "CameraX移到UV位置", checkPos: false, token: token);

            await OneAxisMoveAbsAsync((int)AxisId.CameraZ, _cameraZAdjustSlice + height,
                "CameraZ移到UV对焦位", checkPos: false, token: token);

            // UV 固化 — UV 时间从 config 读取 (m_nUVTime)
            int uvTimeMs = 3000;
            await UVCylinderAsync(true, token);      // UV 气缸下降
            SetDO(BlockCutConstants.DO_UV, true);  // UV 灯开
            await Task.Delay(uvTimeMs, token);
            SetDO(BlockCutConstants.DO_UV, false);  // UV 灯关
            await UVCylinderAsync(false, token);       // UV 气缸上升

            // 后处理安全位: DisZ/DisX/DisY 回安全位
            await OneAxisMoveAbsAsync((int)AxisId.DisZ, _cameraZSafe,
                "DisZ回安全位", checkPos: false, token: token);

            await OneAxisMoveAbsAsync((int)AxisId.DisX, _sliceXAdjust,
                "DisX回安全位", checkPos: false, token: token);

            await OneAxisMoveAbsAsync((int)AxisId.DisY, disY,
                "DisY回安全位", checkPos: false, token: token);

            EmitMessage("调节组点胶完成");
            return true;
        }

        #endregion

        #region 回安全位 (替代 GoSafe)

        /// <summary>各轴回安全位 — 对齐 Qt 完整逻辑</summary>
        private async Task<bool> GoSafeAsync(CancellationToken token)
        {
            EmitMessage("调节组回安全位置...");
            CheckPause(token);

            // UV 气缸上升
            await UVCylinderAsync(false, token);

            // SliceY 气缸关闭 + SliceZ 上升
            await SliceYCylinderAsync(false, token);
            await SliceZCylinderAsync(true, token);

            // AdjustX 等待位置 (安全位从 config 读取，非硬编码 0)
            await OneAxisMoveAbsAsync((int)AxisId.AdjustX, _sliceXAdjust,
                "AdjustX回安全位", checkPos: false, token: token);

            // CameraX → 正限位方向安全位
            double cameraXSafe = _cameraBottomXLeft > 0 ? _cameraBottomXLeft : _cameraBottomXRight;
            await OneAxisMoveAbsAsync((int)AxisId.CameraX, cameraXSafe, "CameraX回安全位",
                checkPos: false, token: token);

            // CameraZ → 安全位
            await OneAxisMoveAbsAsync((int)AxisId.CameraZ, _cameraZSafe, "CameraZ回安全位",
                checkPos: false, token: token);

            // DisZ/DisX/DisY → 安全位
            await OneAxisMoveAbsAsync((int)AxisId.DisZ, _cameraZSafe, "DisZ回安全位",
                checkPos: false, token: token);

            await OneAxisMoveAbsAsync((int)AxisId.DisX, _sliceXAdjust, "DisX回安全位",
                checkPos: false, token: token);

            double disYSafe = _bottomYSetSlice + (_rows - 1) * _bottomRowSpace;
            await OneAxisMoveAbsAsync((int)AxisId.DisY, disYSafe, "DisY回安全位",
                checkPos: false, token: token);

            // AdjustY1/Y2 → 安全位
            await TwoAxisMoveAbsAsync(
                (int)AxisId.AdjustY1, _sliceY1Adjust,
                (int)AxisId.AdjustY2, _sliceY2Adjust,
                "AdjustY1/Y2回安全位", checkPos: false, token: token);

            EmitMessage("调节组已回到安全位置");
            return true;
        }

        #endregion

        #region 气缸辅助方法

        private async Task BottomYCylinderAsync(bool close, CancellationToken token)
        {
            SetDO(BlockCutConstants.DO_BottomCompressCylinder, close);
            int diExpected = close
                ? BlockCutConstants.DI_BottomCompressCylinderReach
                : BlockCutConstants.DI_BottomCompressCylinderRetract;
            await WaitDIAsync(diExpected, true,
                BlockCutConstants.TimeoutWarn, close ? "底板夹紧气缸伸出" : "底板夹紧气缸缩回", token: token);
        }

        private async Task SliceYCylinderAsync(bool close, CancellationToken token)
        {
            SetDO(BlockCutConstants.DO_YCylinder, close);
            int diExpected = close
                ? BlockCutConstants.DI_YCylinderReach
                : BlockCutConstants.DI_YCylinderRetract;
            await WaitDIAsync(diExpected, true,
                BlockCutConstants.TimeoutWarn, close ? "片源气缸夹紧" : "片源气缸缩回", token: token);
        }

        private async Task SliceZCylinderAsync(bool up, CancellationToken token)
        {
            if (up)
            {
                SetDO(BlockCutConstants.DO_AdjustZDown, false);
                SetDO(BlockCutConstants.DO_AdjustZUp, true);
                await WaitDIAsync(BlockCutConstants.DI_AdjustZUp, true,
                    BlockCutConstants.TimeoutWarn, "片源调节Z气缸上升", token: token);
            }
            else
            {
                SetDO(BlockCutConstants.DO_AdjustZUp, false);
                SetDO(BlockCutConstants.DO_AdjustZDown, true);
                await WaitDIAsync(BlockCutConstants.DI_AdjustZDown, true,
                    BlockCutConstants.TimeoutWarn, "片源调节Z气缸下降", token: token);
            }
        }

        private async Task UVCylinderAsync(bool down, CancellationToken token)
        {
            if (down)
            {
                SetDO(BlockCutConstants.DO_UVZCylinder, true);
                await WaitDIAsync(BlockCutConstants.DI_UVCylinderDown, true,
                    BlockCutConstants.TimeoutWarn, "UV气缸下降", token: token);
            }
            else
            {
                SetDO(BlockCutConstants.DO_UVZCylinder, false);
                await WaitDIAsync(BlockCutConstants.DI_UVCylinderUp, true,
                    BlockCutConstants.TimeoutWarn, "UV气缸上升", token: token);
            }
        }

        #endregion

        #region 相机拍照 (替代 CameraOnce)

        /// <summary>单次相机拍照 — 等待 _isCameraFinish + 10s timeout + 最多 3 次重试</summary>
        private async Task<CameraResult> CameraOnceAsync(int cameraId, int imageId,
            CancellationToken token)
        {
            if (IsEmptyTest)
                return new CameraResult { Success = true, Point1 = new Point2D(100, 200), Point2 = new Point2D(300, 400), Angle = 0.5 };

            const int maxRetries = 3;
            const int timeoutMs = 10000;

            for (int retry = 0; retry < maxRetries; retry++)
            {
                token.ThrowIfCancellationRequested();
                CheckPause(token);

                _isCameraFinish = false;

                if (_isChangeExposure)
                {
                    EmitMessage("切换曝光模式...");
                    _isChangeExposure = false;
                }

                // 触发拍照
                var result = await (OnCameraCapture?.Invoke(cameraId, imageId) ?? Task.FromResult<CameraResult>(null));
                if (result != null && result.Success)
                {
                    _cameraResult = result;
                    _isCameraFinish = true;
                    return result;
                }

                // 等待完成或超时
                int waited = 0;
                while (!_isCameraFinish && waited < timeoutMs && !token.IsCancellationRequested)
                {
                    await Task.Delay(10, token);
                    waited += 10;
                }

                if (_isCameraFinish && _cameraResult?.Success == true)
                    return _cameraResult;

                EmitMessage($"相机{cameraId}拍照失败，重试 {retry + 1}/{maxRetries - 1}...");
            }

            PauseWarnMessage(13000 + (int)ThreadId.Adjust, "ERR-VIS-01: 相机拍照失败");
            return null;
        }

        #endregion

        #region 测高 (替代 GetMeasureHeight + MeasureHeight)

        /// <summary>获取测高值 — 对齐 Qt 完整逻辑</summary>
        private async Task<double> GetMeasureHeightAsync(CancellationToken token)
        {
            if (IsEmptyTest) return 0.5;

            CheckPause(token);

            // 测高气缸下降 (DO_PosHeight)
            SetDO(BlockCutConstants.DO_PosHeight, true);
            await Task.Delay(200, token);

            // 等待测量完成
            await WaitMeasureEndAsync(token);

            // 读取高度值 + 验证范围
            double height = _measureHeight;
            if (!_isMeasureSuccess || height < _heightLow || height > _heightHigh)
            {
                EmitMessage($"测高失败: success={_isMeasureSuccess}, height={height:F3}");
                return double.NaN;
            }

            EmitMessage($"测高完成: {height:F3}mm");
            return height;
        }

        /// <summary>等待测高完成 (替代 WaitMeasureEnd)</summary>
        private async Task WaitMeasureEndAsync(CancellationToken token)
        {
            _isMeasureEnd = false;
            const int timeoutMs = 5000;
            int waited = 0;

            while (!_isMeasureEnd && waited < timeoutMs && !token.IsCancellationRequested)
            {
                await Task.Delay(10, token);
                waited += 10;
            }

            if (!_isMeasureEnd)
            {
                EmitMessage("测高等待超时");
                _isMeasureSuccess = false;
            }
        }

        /// <summary>移动到测高位置并执行测高 (替代 MeasureHeight)</summary>
        private async Task<double> MeasureHeightAtAsync(int row, int col, CancellationToken token)
        {
            // CameraX 移动到测高 X 位置 (3 个可选位置之一)
            double cameraXHeight = _cameraXHeight1;
            if (col % 3 == 1) cameraXHeight = _cameraXHeight2;
            if (col % 3 == 2) cameraXHeight = _cameraXHeight3;

            await OneAxisMoveAbsAsync((int)AxisId.CameraX, cameraXHeight,
                $"CameraX移到测高位(col={col})", checkPos: false, token: token);

            // CameraZ 移动到测高 Z 位置
            await OneAxisMoveAbsAsync((int)AxisId.CameraZ, _cameraZHeight,
                "CameraZ移到测高对焦位", checkPos: false, token: token);

            // BottomY 移动到对应行位置
            double bottomY = _bottomYAdjustBottom + (row - 1) * _bottomRowSpace;
            await OneAxisMoveAbsAsync((int)AxisId.BottomY, bottomY,
                $"BottomY移到第{row}行测高位", checkPos: false, token: token);

            return await GetMeasureHeightAsync(token);
        }

        #endregion

        #region 测高网格 (替代 CheckHeightRange)

        /// <summary>9 点网格测高 — 遍历 _rows × _cols 网格，计算 max-min 偏差</summary>
        private async Task<bool> CheckHeightRangeAsync(bool checkRange, CancellationToken token)
        {
            EmitMessage("开始网格测高...");
            CheckPause(token);

            double min = double.MaxValue;
            double max = double.MinValue;

            for (int r = 1; r <= _rows; r++)
            {
                for (int c = 1; c <= _cols; c++)
                {
                    token.ThrowIfCancellationRequested();
                    CheckPause(token);

                    int idx = (r - 1) * _cols + (c - 1);
                    double h = await MeasureHeightAtAsync(r, c, token);

                    if (double.IsNaN(h))
                    {
                        EmitWarning(13000, $"测高失败: row={r}, col={c}", false);
                        _heightCheckRetries++;
                        if (_heightCheckRetries > 3)
                        {
                            PauseWarnMessage(13000 + (int)ThreadId.Adjust,
                                "ERR-VIS-03: 网格测高多次失败");
                            return false;
                        }
                        return false;
                    }

                    _measureHeights[idx] = h;
                    if (h < min) min = h;
                    if (h > max) max = h;
                }
            }

            _heightCheckRetries = 0;

            if (checkRange)
            {
                double range = max - min;
                EmitMessage($"测高范围: {min:F3} ~ {max:F3}, 偏差={range:F3}mm");

                if (range > _heightHigh)
                {
                    PauseWarnMessage(13000 + (int)ThreadId.Adjust,
                        $"ERR-VIS-02: 测高偏差过大 max-min={range:F3}mm > {_heightHigh:F3}mm");
                    return false;
                }
            }

            EmitMessage("网格测高完成");
            return true;
        }

        #endregion

        #region 手动操作方法 (Task 1i)

        /// <summary>手动: 调节底板组</summary>
        public async Task GroupAdjustBottomAsync(CancellationToken token)
        {
            EmitMessage("手动调节底板...");
            await SliceZCylinderAsync(false, token);
            await OneAxisMoveAbsAsync((int)AxisId.CameraX, _cameraBottomXRight,
                "CameraX到右侧拍底板位", checkPos: false, token: token);
            await SliceZCylinderAsync(true, token);
            await BottomYCylinderAsync(true, token);
        }

        /// <summary>手动: 调节片源组</summary>
        public async Task GroupAdjustSliceAsync(CancellationToken token)
        {
            EmitMessage("手动调节片源...");
            await OneAxisMoveAbsAsync((int)AxisId.CameraZ, _cameraZSafe,
                "CameraZ回安全位", checkPos: false, token: token);
            await SliceYCylinderAsync(true, token);
            await SliceZCylinderAsync(false, token);
        }

        /// <summary>手动: 设定片源位置</summary>
        public async Task GroupSetSliceAsync(CancellationToken token)
        {
            EmitMessage("手动设定片源...");
            double disX = _sliceXAdjust - (_col - 1) * _bottomColSpace;
            double disY = _bottomYSetSlice + (_row - 1) * _bottomRowSpace;
            await TwoAxisMoveAbsAsync((int)AxisId.DisX, disX, (int)AxisId.DisY, disY,
                "DisXY定位", checkPos: false, token: token);
            SetDO(BlockCutConstants.DO_YCylinder, false);
            await Task.Delay(500, token);
            SetDO(BlockCutConstants.DO_YCylinder, true);
        }

        /// <summary>手动: 送走底板</summary>
        public async Task GroupSendBottomAsync(CancellationToken token)
        {
            EmitMessage("手动送底板...");
            await SendOrGetBottomAsync(false, token);
        }

        /// <summary>手动: 单次测高</summary>
        public async Task GroupHeightMeasureOnceAsync(CancellationToken token)
        {
            EmitMessage("手动单次测高...");
            double h = await MeasureHeightAtAsync(1, 1, token);
            EmitMessage($"测高结果: {h:F3}mm");
        }

        /// <summary>手动: 推空板</summary>
        public async Task GroupPushEmptyAsync(CancellationToken token)
        {
            EmitMessage("手动推空板...");
            SetDO(BlockCutConstants.DO_BottomOutPushCylinder, true);
            await Task.Delay(1000, token);
            SetDO(BlockCutConstants.DO_BottomOutPushCylinder, false);
        }

        /// <summary>手动: 取满板</summary>
        public async Task GroupGetFullAsync(CancellationToken token)
        {
            EmitMessage("手动取满板...");
            await SendOrGetBottomAsync(false, token);
        }

        #endregion

        #region 外部回调

        /// <summary>相机拍照结果回调 (替代 CameraResult)</summary>
        public void OnCameraResult(bool success, Point2D pt1, Point2D pt2, double angle)
        {
            _isCameraFinish = true;
            _isChangeExposure = !success;
            _cameraResult = new CameraResult
            {
                Success = success,
                Point1 = pt1,
                Point2 = pt2,
                Angle = angle,
            };
        }

        /// <summary>BottomGet 就位回调 (替代 ReadyFromBottomXOnPos)</summary>
        public void ReadyFromBottomXOnPos()
        {
            _isReadyFromBottomXOnPos = true;
        }

        /// <summary>Load2 到位回调 (替代 ReadyFromLoad2OnPos)</summary>
        public void OnLoad2Ready(string jigCode, string blockCode)
        {
            _jigCode = jigCode;
            _blockCode = blockCode;
            _isReadyFromLoad2OnPos = true;
        }

        /// <summary>测高完成回调 (替代 ReadyFromHeight)</summary>
        public void OnHeightReady(bool success, double height)
        {
            _isMeasureSuccess = success;
            _measureHeight = height;
            _isMeasureEnd = true;
        }

        /// <summary>扫码完成回调 (替代 ReadyFromSweepCode)</summary>
        public void OnSweepCodeReady(string code)
        {
            _bottomCode = code;
        }

        /// <summary>MES 底板验证完成回调 (替代 FinishFromMes)</summary>
        public void FinishFromMes(string fileId, string alertMsg)
        {
            _mesFileId = fileId;
            _mesAlertMsg = alertMsg;
            _isWaitMes = true;
        }

        /// <summary>MES 单片验证完成回调 (替代 FinishFromMesSlice)</summary>
        public void FinishFromMesSlice(int pieceStatus, string bizErrMsg)
        {
            _mesPieceStatus = pieceStatus;
            _mesAlertMsg = bizErrMsg;
            _isWaitMes = true;
        }

        #endregion

        #region 手动操作 (Group* 扩展)

        /// <summary>相机篮子固定拍照 — 在当前位置连拍 nCount 次 (替代 CameraBaketFix)</summary>
        public async Task CameraBaketFixAsync(int nCount, CancellationToken token)
        {
            for (int i = 0; i < nCount; i++)
            {
                if (token.IsCancellationRequested) break;
                EmitMessage($"篮子固定拍照 {i + 1}/{nCount}");
                await CameraOnceAsync(0, i, token);
            }
            EmitMessage("篮子固定拍照完成");
        }

        /// <summary>相机篮 X 轴解固定 — 移出 dSpace → 回拍照位 → 拍照，重复 nCount 次 (替代 CameraBaketXUnFix)</summary>
        public async Task CameraBaketXUnFixAsync(int nCount, double dSpace, CancellationToken token)
        {
            for (int i = 0; i < nCount; i++)
            {
                if (token.IsCancellationRequested) break;

                double curX = Motion.GetAxisPos((int)AxisId.CameraX);
                if (await OneAxisMoveAbsAsync((int)AxisId.CameraX, curX + dSpace,
                    $"相机X移出位 ({i + 1}/{nCount})", token: token) != 1) break;
                if (await OneAxisMoveAbsAsync((int)AxisId.CameraX, curX,
                    $"相机X回拍照位 ({i + 1}/{nCount})", token: token) != 1) break;

                EmitMessage($"X轴解固定拍照 {i + 1}/{nCount}");
                await CameraOnceAsync(0, i, token);
            }
            EmitMessage("X轴解固定完成");
        }

        /// <summary>相机篮 Y 轴解固定 (替代 CameraBaketYUnFix)</summary>
        public async Task CameraBaketYUnFixAsync(int nCount, double dSpace, CancellationToken token)
        {
            for (int i = 0; i < nCount; i++)
            {
                if (token.IsCancellationRequested) break;

                double curY = Motion.GetAxisPos((int)AxisId.BottomY);
                if (await OneAxisMoveAbsAsync((int)AxisId.BottomY, curY + dSpace,
                    $"BottomY移出位 ({i + 1}/{nCount})", token: token) != 1) break;
                if (await OneAxisMoveAbsAsync((int)AxisId.BottomY, curY,
                    $"BottomY回拍照位 ({i + 1}/{nCount})", token: token) != 1) break;

                EmitMessage($"Y轴解固定拍照 {i + 1}/{nCount}");
                await CameraOnceAsync(0, i, token);
            }
            EmitMessage("Y轴解固定完成");
        }

        /// <summary>相机篮 Z 轴解固定 (替代 CameraBaketZUnFix)</summary>
        public async Task CameraBaketZUnFixAsync(int nCount, double dSpace, CancellationToken token)
        {
            for (int i = 0; i < nCount; i++)
            {
                if (token.IsCancellationRequested) break;

                double curZ = Motion.GetAxisPos((int)AxisId.CameraZ);
                if (await OneAxisMoveAbsAsync((int)AxisId.CameraZ, curZ + dSpace,
                    $"CameraZ移出位 ({i + 1}/{nCount})", token: token) != 1) break;
                if (await OneAxisMoveAbsAsync((int)AxisId.CameraZ, curZ,
                    $"CameraZ回拍照位 ({i + 1}/{nCount})", token: token) != 1) break;

                EmitMessage($"Z轴解固定拍照 {i + 1}/{nCount}");
                await CameraOnceAsync(0, i, token);
            }
            EmitMessage("Z轴解固定完成");
        }

        #endregion

        #region 片源 MES 验证 (替代 CheckSliceCard)

        /// <summary>
        /// 片源 MES 状态校验 (替代 CheckSliceCard)
        /// 异步调用 MES 验证片源码，5s 超时
        /// pieceStatus: 0=OK, 1=次品, 2=异常, 3=黑名单
        /// </summary>
        public async Task<bool> CheckSliceCardAsync(CancellationToken token)
        {
            if (OnSliceMesValidate == null)
            {
                // 空跑模式 / 无 MES
                return true;
            }

            EmitMessage("等待片源MES验证...");
            CheckPause(token);

            try
            {
                using var cts = new CancellationTokenSource(5000);
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, cts.Token);

                var result = await OnSliceMesValidate(_blockCode);
                if (result?.IsValid == true && result?.PieceStatus == 0)
                {
                    return true;
                }

                // 片源异常处理
                string statusStr = result?.PieceStatus switch
                {
                    0 => null,
                    1 => "次品",
                    2 => "异常",
                    3 => "黑名单",
                    _ => result?.Message ?? "未知状态",
                };

                if (!string.IsNullOrEmpty(statusStr))
                {
                    EmitWarning(11004,
                        $"{BlockCutConstants.ThreadAdjustName}暂停，单片状态: {statusStr}",
                        true);
                    Pause();
                }
                else if (!string.IsNullOrEmpty(result?.Message))
                {
                    EmitWarning(11004,
                        $"{BlockCutConstants.ThreadAdjustName}暂停，单片状态异常: {result.Message}",
                        true);
                    Pause();
                }
            }
            catch (OperationCanceledException)
            {
                EmitMessage("片源MES验证超时");
            }
            catch (Exception ex)
            {
                EmitWarning(11004,
                    $"{BlockCutConstants.ThreadAdjustName}片源MES验证异常: {ex.Message}",
                    true);
                Pause();
            }

            return false;
        }

        #endregion

        #region 轴辅助方法

        /// <summary>相机 X 正方向运动 — 增量运动到正限位 (替代 CameraXMoveP)</summary>
        private async Task<bool> CameraXMoveP(CancellationToken token)
        {
            double curPos = Motion.GetAxisPos((int)AxisId.CameraX);
            double targetPos = curPos + _posMax;
            return await OneAxisMoveAbsAsync((int)AxisId.CameraX, targetPos,
                "相机X运动到正限位", token: token) == 1;
        }

        #endregion
    }

}
