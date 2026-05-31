using System;
using System.Collections.Generic;
using OmniFrame.Common;
using OmniFrame.Core.AdvancedFeatures;

namespace OmniFrame.Core.BlockCut
{
    /// <summary>
    /// 缺陷信息 — 用于模拟缺陷注入测试
    /// </summary>
    public class DefectInfo
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public double Severity { get; set; } = 1.0;  // 0=无, 1=轻微, 2=严重
        public Point2D Position { get; set; }
        public double Angle { get; set; }
        public DefectType Type { get; set; } = DefectType.None;
    }

    /// <summary>缺陷类型</summary>
    public enum DefectType
    {
        None,
        Scratch,
        Crack,
        Bubble,
        Contamination,
        MissingEdge,
        AngleOffset
    }

    /// <summary>
    /// Halcon .NET 视觉系统 — 实现 IVisionSystem，整合 BlockCut 专用视觉算法。
    /// 替代 CFitLineAlg + FitLineWidget + CameraWidget 视觉处理逻辑。
    ///
    /// IVisionSystem 接口方法用于通用检测流程；
    /// Halcon 专用方法 (FitLine / MeasureHeight / DetectAngle / AutoExposure) 供 BlockCut 工站调用。
    ///
    /// 仿真模式: 通过 SimulatedAngle / SimulatedMaxGray 等属性控制模拟输出，
    /// 并支持 SetSimulatedDefect() 注入缺陷供测试使用。
    /// </summary>
    public class BlockCutVision : IVisionSystem
    {
        private bool _isInitialized;
        private bool _isEmptyTest;
        private readonly Random _rng = new Random();
        private readonly List<DefectInfo> _simulatedDefects = new List<DefectInfo>();

        // ROI 参数
        public ROIRegion SearchRegion { get; set; }
        public FitLineParams FitParams { get; set; } = new FitLineParams();

        // ---- 可配置模拟参数 ----
        /// <summary>仿真角度 (度)</summary>
        public double SimulatedAngle { get; set; } = 0.5;
        /// <summary>仿真最大灰度值</summary>
        public double SimulatedMaxGray { get; set; } = 200;
        /// <summary>仿真平均灰度值</summary>
        public double SimulatedAvgGray { get; set; } = 128;

        /// <summary>空跑模式 — 返回硬编码值，不调用真实 Halcon</summary>
        public bool IsEmptyTest
        {
            get => _isEmptyTest;
            set
            {
                _isEmptyTest = value;
                Logger.Info($"[BlockCutVision] 空跑模式: {(_isEmptyTest ? "ON" : "OFF")}");
            }
        }

        /// <summary>获取当前注入的模拟缺陷列表</summary>
        public IReadOnlyList<DefectInfo> SimulatedDefects => _simulatedDefects.AsReadOnly();

        /// <summary>注入一个模拟缺陷</summary>
        public void SetSimulatedDefect(DefectInfo defect)
        {
            if (defect == null) throw new ArgumentNullException(nameof(defect));
            _simulatedDefects.Add(defect);
            Logger.Info($"[BlockCutVision] 注入模拟缺陷: [{defect.Type}] {defect.Code} - {defect.Description}");
        }

        /// <summary>清除所有注入的模拟缺陷</summary>
        public void ClearSimulatedDefects()
        {
            _simulatedDefects.Clear();
            Logger.Info("[BlockCutVision] 已清除所有模拟缺陷");
        }

        #region IVisionSystem 实现

        /// <summary>Halcon 为本地库，Connect 仅初始化 License</summary>
        public bool Connect(string ip, int port)
        {
            try
            {
                Logger.Info($"[BlockCutVision] Halcon 引擎初始化: ip={ip}, port={port}");
                _isInitialized = true;
                IsConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("[BlockCutVision] Halcon 初始化失败", ex);
                IsConnected = false;
                return false;
            }
        }

        /// <summary>释放 Halcon 资源</summary>
        public void Disconnect()
        {
            _isInitialized = false;
            IsConnected = false;
            Logger.Info("[BlockCutVision] Halcon 引擎已释放");
        }

        public bool IsConnected { get; private set; }

        /// <summary>
        /// 通用视觉检测 — 供主流程/非 BlockCut 工站调用。
        /// 内部委托给 FitLine，将结果映射为 VisionResult。
        /// </summary>
        public VisionResult Detect(VisionParams parameters)
        {
            Logger.Info($"[BlockCutVision] Detect: Station={parameters.StationName}, Product={parameters.ProductId}, Exposure={parameters.ExposureTime}");

            if (!_isInitialized)
                return new VisionResult { IsPass = false, Score = 0, Message = "Halcon 未初始化" };

            // 检查是否有注入缺陷影响本次检测
            bool hasDefect = _simulatedDefects.Exists(d => d.Type != DefectType.None);
            if (hasDefect)
            {
                var defect = _simulatedDefects.Find(d => d.Type == DefectType.AngleOffset);
                if (defect != null)
                {
                    return new VisionResult
                    {
                        IsPass = defect.Severity < 1.0,
                        Score = defect.Severity < 1.0 ? 1.0 : 0.3,
                        Message = $"检测到缺陷: [{defect.Type}] {defect.Description}"
                    };
                }

                return new VisionResult
                {
                    IsPass = false,
                    Score = 0.3,
                    Message = $"检测到缺陷: {_simulatedDefects.Count} 个"
                };
            }

            bool ok = FitLine(null, SearchRegion, FitParams, out _, out _, out double angle);
            return new VisionResult
            {
                IsPass = ok,
                Score = ok ? 1.0 : 0,
                Message = ok ? $"检测通过, 角度={angle:F2}°" : "线拟合失败",
            };
        }

        #endregion

        #region 线拟合 (替代 FitLineAlg::AutoRun)

        /// <summary>
        /// 边缘检测 + 线拟合 — Halcon .NET API 调用链:
        ///   gen_rectangle1 → reduce_domain → edges_sub_pix
        ///   → segment_contours_xld → select_shape_xld → fit_line_contour_xld
        ///
        /// 空跑/仿真模式: 根据 SimulatedAngle + 随机噪声返回模拟值
        /// </summary>
        public bool FitLine(object image, ROIRegion roi, FitLineParams @params,
            out Point2D p1, out Point2D p2, out double angle)
        {
            Logger.Info($"[BlockCutVision] FitLine: IsEmptyTest={_isEmptyTest}, IsInit={_isInitialized}, roi={roi?.CenterX},{roi?.CenterY}");

            if (!_isInitialized) { p1 = p2 = default; angle = 0; return false; }

            if (_isEmptyTest || !IsConnected)
            {
                // 仿真模式: 基于 SimulatedAngle 生成一致但有噪声的结果
                double noiseDeg = (_rng.NextDouble() - 0.5) * 0.2;  // ±0.1°
                angle = SimulatedAngle + noiseDeg;

                p1 = new Point2D(100, 200);
                p2 = new Point2D(300, 200 + Math.Tan(angle * Math.PI / 180.0) * 200);

                Logger.Info($"[BlockCutVision] FitLine (sim): angle={angle:F3}°, p1={p1}, p2={p2}");
                return true;
            }

            try
            {
                // Halcon .NET 集成调用链 (待硬件接入):
                // ...
                // 待硬件接入: 暂时返回模拟值
                double noiseDeg2 = (_rng.NextDouble() - 0.5) * 0.2;
                angle = SimulatedAngle + noiseDeg2;
                p1 = new Point2D(100, 200);
                p2 = new Point2D(300, 200 + Math.Tan(angle * Math.PI / 180.0) * 200);
                Logger.Info($"[BlockCutVision] FitLine (stub): angle={angle:F3}°");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("[BlockCutVision] FitLine 失败", ex);
                p1 = p2 = default;
                angle = 0;
                return false;
            }
        }

        /// <summary>在给定图像上做线拟合并返回角度</summary>
        public double DetectAngle(object image, ROIRegion roi)
        {
            Logger.Info($"[BlockCutVision] DetectAngle called: IsEmptyTest={_isEmptyTest}");
            if (_isEmptyTest || !IsConnected)
            {
                double noise = (_rng.NextDouble() - 0.5) * 0.2;
                double result = SimulatedAngle + noise;
                Logger.Info($"[BlockCutVision] DetectAngle (sim): {result:F3}°");
                return result;
            }
            return FitLine(image, roi, FitParams, out _, out _, out double angle) ? angle : double.NaN;
        }

        /// <summary>检测缺陷列表 (仿真时可返回注入的缺陷)</summary>
        public List<DefectInfo> DetectDefects(object image)
        {
            Logger.Info($"[BlockCutVision] DetectDefects called: defects={_simulatedDefects.Count}");
            if (_simulatedDefects.Count > 0)
            {
                // 返回注入缺陷的副本
                return new List<DefectInfo>(_simulatedDefects);
            }
            return new List<DefectInfo>(); // 默认无缺陷
        }

        #endregion

        #region 灰度统计 (替代 GetMaxGray / GetGray)

        /// <summary>图像最大灰度值 — Halcon: min_max_gray</summary>
        public int GetMaxGray(object image)
        {
            Logger.Info($"[BlockCutVision] GetMaxGray called");
            if (_isEmptyTest || !IsConnected)
            {
                int noise = _rng.Next(-10, 11); // ±10
                int result = Math.Max(0, Math.Min(255, (int)SimulatedMaxGray + noise));
                Logger.Info($"[BlockCutVision] GetMaxGray (sim): {result}");
                return result;
            }

            try
            {
                // Halcon 调用 (待硬件接入)
                return 255;
            }
            catch (Exception ex)
            {
                Logger.Error("[BlockCutVision] GetMaxGray 失败", ex);
                return 0;
            }
        }

        /// <summary>图像平均灰度值 — Halcon: intensity</summary>
        public double GetAvgGray(object image)
        {
            Logger.Info($"[BlockCutVision] GetAvgGray called");
            if (_isEmptyTest || !IsConnected)
            {
                double noise = (_rng.NextDouble() - 0.5) * 10; // ±5
                double result = Math.Max(0, Math.Min(255, SimulatedAvgGray + noise));
                Logger.Info($"[BlockCutVision] GetAvgGray (sim): {result:F1}");
                return result;
            }

            try
            {
                // Halcon 调用 (待硬件接入)
                return 128.0;
            }
            catch (Exception ex)
            {
                Logger.Error("[BlockCutVision] GetAvgGray 失败", ex);
                return 0;
            }
        }

        #endregion

        #region 自动曝光 (替代 AutoRun + 对数曝光算法)

        /// <summary>
        /// 自动曝光算法 — 对数曝光模型 + 目标灰度 180-220
        /// 公式: newExp = curExp * log(target) / log(curGray)
        /// 范围限制: [100μs, 500000μs]
        /// </summary>
        public double AdjustExposure(int currentMaxGray, double currentExposureUs)
        {
            Logger.Info($"[BlockCutVision] AdjustExposure: gray={currentMaxGray}, exp={currentExposureUs:F0}μs");
            if (_isEmptyTest) return currentExposureUs;

            const int targetMin = 180, targetMax = 220, targetMid = 200;

            if (currentMaxGray >= targetMin && currentMaxGray <= targetMax)
            {
                Logger.Info($"[BlockCutVision] 曝光正常，无需调整");
                return currentExposureUs;
            }

            // 对数曝光模型
            double target = currentMaxGray < targetMin ? targetMid + 20 : targetMid - 20;
            double ratio = Math.Log(Math.Max(target, 1)) / Math.Log(Math.Max(currentMaxGray, 1));
            double newExp = currentExposureUs * ratio;
            double adjusted = Math.Max(100, Math.Min(500000, newExp));

            Logger.Info($"[BlockCutVision] 曝光调整: {currentExposureUs:F0} → {adjusted:F0}μs (ratio={ratio:F3})");
            return adjusted;
        }

        #endregion

        #region 高度测量 (替代 MeasureHeight + 9 点测高)

        /// <summary>
        /// 单点激光测高/对焦 — Halcon: depth_from_focus 或激光三角法
        /// 空跑/仿真模式返回典型值 0.5mm ± 噪声
        /// </summary>
        public double MeasureHeightPoint(object image, ROIRegion roi)
        {
            Logger.Info($"[BlockCutVision] MeasureHeightPoint: roi=({roi?.CenterX},{roi?.CenterY})");
            if (_isEmptyTest || !IsConnected)
            {
                double noise = (_rng.NextDouble() - 0.5) * 0.02; // ±0.01mm
                double result = 0.5 + noise;
                Logger.Info($"[BlockCutVision] MeasureHeightPoint (sim): {result:F4}mm");
                return result;
            }

            try
            {
                // Halcon 测高调用 (待硬件接入)
                return 0.5;
            }
            catch (Exception ex)
            {
                Logger.Error("[BlockCutVision] MeasureHeightPoint 失败", ex);
                return double.NaN;
            }
        }

        /// <summary>9 点网格测高</summary>
        public double[] MeasureHeightGrid(object[] images, ROIRegion[] rois)
        {
            Logger.Info($"[BlockCutVision] MeasureHeightGrid: {images?.Length ?? 0} images");
            if (_isEmptyTest || !IsConnected)
            {
                var dummy = new double[9];
                for (int i = 0; i < 9; i++)
                {
                    double noise = (_rng.NextDouble() - 0.5) * 0.02;
                    dummy[i] = 0.5 + noise;
                }
                return dummy;
            }

            var h = new double[9];
            for (int i = 0; i < 9 && i < images.Length; i++)
                h[i] = MeasureHeightPoint(images[i], rois[i]);
            return h;
        }

        #endregion
    }

    #region 视觉参数类型

    /// <summary>线拟合参数 (替代 FitLineWidget 参数面板)</summary>
    public class FitLineParams
    {
        public double Sigma { get; set; } = 1.0;
        public int Threshold { get; set; } = 50;
        public int Transition { get; set; } = 0;     // all / positive / negative
        public int Select { get; set; } = 0;         // first / last / all
        public int FitAlg { get; set; } = 0;         // Tukey / Huber / LeastSquares
        public int SegWidth { get; set; } = 20;
        public int SegStep { get; set; } = 30;
        public int SegNum { get; set; } = 10;
        public int Interpolation { get; set; } = 1;  // nearest / bilinear / bicubic
        public int MaxNumPoints { get; set; } = 500;
        public int Iterations { get; set; } = 5;
        public double DistanceFactor { get; set; } = 1.0;
    }

    /// <summary>ROI 区域 (替代 Base::sRegion)</summary>
    public class ROIRegion
    {
        public RegionType Type { get; set; } = RegionType.Rect;

        public double CenterX { get; set; } = 50;
        public double CenterY { get; set; } = 50;
        public double Width { get; set; } = 100;
        public double Height { get; set; } = 100;

        public double Angle { get; set; }
        public double Radius { get; set; } = 50;
        public double InnerRadius { get; set; } = 25;
        public double OuterRadius { get; set; } = 50;
        public double StartAngle { get; set; }
        public double AngleExtent { get; set; } = 360;

        public int SegWidth { get; set; } = 20;
        public int SegStep { get; set; } = 30;

        public List<Point2D> Polygon { get; set; } = new List<Point2D>();

        // 标定
        public double FixtureX { get; set; }
        public double FixtureY { get; set; }
        public double FixtureAngle { get; set; }
    }

    #endregion
}
