using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using OmniFrame.Common;

namespace OmniFrame.Core
{
    /// <summary>
    /// 配方参数
        /// </summary>
    public class RecipeParameter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
        public string DefaultValue { get; set; }
        public string DataType { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public string Unit { get; set; }
    }

    /// <summary>
    /// 运动参数
        /// </summary>
    public class MotionParameter
    {
        public string AxisName { get; set; }
        public double Velocity { get; set; }
        public double Acceleration { get; set; }
        public double Deceleration { get; set; }
        public double HomeVelocity { get; set; }
        public double HomeOffset { get; set; }
        public double SoftLimitPositive { get; set; }
        public double SoftLimitNegative { get; set; }
        public bool EnableSoftLimit { get; set; }
    }

    /// <summary>
    /// 位置点
        /// </summary>
    public class PositionPoint
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, double> AxisPositions { get; set; }
        public double Velocity { get; set; }
        public bool IsRelative { get; set; }

        public PositionPoint()
        {
            AxisPositions = new Dictionary<string, double>();
        }
    }

    /// <summary>
    /// 配方数据
        /// </summary>
    public class RecipeData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProductModel { get; set; }
        public string Version { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? ModifyTime { get; set; }
        public string Author { get; set; }
        public bool IsDefault { get; set; }

        // 参数集合
        public List<RecipeParameter> Parameters { get; set; }
        public List<MotionParameter> MotionParams { get; set; }
        public List<PositionPoint> PositionPoints { get; set; }

        public RecipeData()
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            CreateTime = DateTime.Now;
            Version = "1.0";
            Parameters = new List<RecipeParameter>();
            MotionParams = new List<MotionParameter>();
            PositionPoints = new List<PositionPoint>();
        }

        /// <summary>
        /// 获取参数值
        /// </summary>
        public string GetParameter(string name, string defaultValue = null)
        {
            var param = Parameters.FirstOrDefault(p => p.Name == name);
            return param?.Value ?? defaultValue;
        }

        /// <summary>
        /// 设置参数值
        /// </summary>
        public void SetParameter(string name, string value)
        {
            var param = Parameters.FirstOrDefault(p => p.Name == name);
            if (param != null)
            {
                param.Value = value;
            }
            else
            {
                Parameters.Add(new RecipeParameter
                {
                    Name = name,
                    Value = value
                });
            }
        }

        /// <summary>
        /// 获取运动参数
        /// </summary>
        public MotionParameter GetMotionParameter(string axisName)
        {
            return MotionParams.FirstOrDefault(p => p.AxisName == axisName);
        }

        /// <summary>
        /// 获取位置点
        /// </summary>
        public PositionPoint GetPositionPoint(string name)
        {
            return PositionPoints.FirstOrDefault(p => p.Name == name);
        }
    }

    /// <summary>
    /// 配方管理器
        /// </summary>
    public class RecipeManager : IDisposable, IRecipeManager
    {
        private readonly object _lock = new object();
        private List<RecipeData> _recipes;
        private string _recipeFolder;
        private RecipeData _currentRecipe;
        private bool _isDisposed;


        public RecipeData CurrentRecipe => _currentRecipe;
        public int RecipeCount => _recipes.Count;
        public List<RecipeData> AllRecipes => _recipes.ToList();

        public event EventHandler<RecipeData> RecipeLoaded;
        public event EventHandler<RecipeData> RecipeSaved;
        public event EventHandler<RecipeData> RecipeDeleted;
        public event EventHandler<RecipeData> CurrentRecipeChanged;

        public RecipeManager()
        {
            _recipes = new List<RecipeData>();
            _recipeFolder = "Recipes";
        }

        public bool Initialize()
        {
            try
            {
                Logger.Info("初始化配方管理器...");

                // 确保配方目录存在
                if (!Directory.Exists(_recipeFolder))
                {
                    Directory.CreateDirectory(_recipeFolder);
                }

                // 加载所有配方
                LoadAllRecipes();

                // 如果没有配方，创建默认配方
                if (_recipes.Count == 0)
                {
                    CreateDefaultRecipe();
                }

                Logger.Info($"配方管理器初始化完成，共 {_recipes.Count} 个配方");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("配方管理器初始化失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 创建默认配方
        /// </summary>
        private void CreateDefaultRecipe()
        {
            var recipe = new RecipeData
            {
                Name = "BLOCKCUT-STD",
                Description = "BlockCut 标准生产配方 — 适用于标准尺寸产品",
                ProductModel = "BC-V1",
                IsDefault = true,
                Author = "System"
            };

            // BlockCut 核心工艺参数
            recipe.Parameters.Add(new RecipeParameter { Name = "DispenseTimeMs", Description = "点胶时间", Value = "500", DefaultValue = "500", DataType = "Int", MinValue = "100", MaxValue = "2000", Unit = "ms" });
            recipe.Parameters.Add(new RecipeParameter { Name = "UvCureTimeMs", Description = "UV固化时间", Value = "3000", DefaultValue = "3000", DataType = "Int", MinValue = "500", MaxValue = "10000", Unit = "ms" });
            recipe.Parameters.Add(new RecipeParameter { Name = "CameraExposureMs", Description = "相机曝光时间", Value = "50", DefaultValue = "50", DataType = "Int", MinValue = "10", MaxValue = "500", Unit = "ms" });
            recipe.Parameters.Add(new RecipeParameter { Name = "CameraGain", Description = "相机增益", Value = "1.0", DefaultValue = "1.0", DataType = "Double", MinValue = "0.5", MaxValue = "4.0", Unit = "x" });
            recipe.Parameters.Add(new RecipeParameter { Name = "InspectionAngleTolerance", Description = "检测角度容差", Value = "0.5", DefaultValue = "0.5", DataType = "Double", MinValue = "0.1", MaxValue = "5.0", Unit = "°" });
            recipe.Parameters.Add(new RecipeParameter { Name = "MaxGrayThreshold", Description = "最大灰度阈值", Value = "200", DefaultValue = "200", DataType = "Int", MinValue = "50", MaxValue = "255", Unit = "" });
            recipe.Parameters.Add(new RecipeParameter { Name = "PickupDelayMs", Description = "取料延迟", Value = "200", DefaultValue = "200", DataType = "Int", MinValue = "50", MaxValue = "1000", Unit = "ms" });
            recipe.Parameters.Add(new RecipeParameter { Name = "PlaceDelayMs", Description = "放料延迟", Value = "300", DefaultValue = "300", DataType = "Int", MinValue = "50", MaxValue = "1000", Unit = "ms" });

            // 运动轴参数 (BlockCut 8轴)
            recipe.MotionParams.Add(new MotionParameter { AxisName = "X", Velocity = 200, Acceleration = 2000, Deceleration = 2000, HomeVelocity = 50, EnableSoftLimit = true, SoftLimitPositive = 400, SoftLimitNegative = 0 });
            recipe.MotionParams.Add(new MotionParameter { AxisName = "Y", Velocity = 200, Acceleration = 2000, Deceleration = 2000, HomeVelocity = 50, EnableSoftLimit = true, SoftLimitPositive = 350, SoftLimitNegative = 0 });
            recipe.MotionParams.Add(new MotionParameter { AxisName = "Z", Velocity = 100, Acceleration = 1000, Deceleration = 1000, HomeVelocity = 30, EnableSoftLimit = true, SoftLimitPositive = 200, SoftLimitNegative = -10 });
            recipe.MotionParams.Add(new MotionParameter { AxisName = "U", Velocity = 150, Acceleration = 1500, Deceleration = 1500, HomeVelocity = 40, EnableSoftLimit = true, SoftLimitPositive = 360, SoftLimitNegative = 0 });
            recipe.MotionParams.Add(new MotionParameter { AxisName = "A1", Velocity = 100, Acceleration = 800, Deceleration = 800, HomeVelocity = 30, EnableSoftLimit = true, SoftLimitPositive = 180, SoftLimitNegative = 0 });
            recipe.MotionParams.Add(new MotionParameter { AxisName = "A2", Velocity = 100, Acceleration = 800, Deceleration = 800, HomeVelocity = 30, EnableSoftLimit = true, SoftLimitPositive = 180, SoftLimitNegative = 0 });
            recipe.MotionParams.Add(new MotionParameter { AxisName = "B1", Velocity = 80, Acceleration = 500, Deceleration = 500, HomeVelocity = 20, EnableSoftLimit = false, SoftLimitPositive = 0, SoftLimitNegative = 0 });
            recipe.MotionParams.Add(new MotionParameter { AxisName = "B2", Velocity = 80, Acceleration = 500, Deceleration = 500, HomeVelocity = 20, EnableSoftLimit = false, SoftLimitPositive = 0, SoftLimitNegative = 0 });

            // 位置点 (BlockCut 关键点位)
            recipe.PositionPoints.Add(CreatePosPoint("Home", "原点位置", 50, (0,0,0)));
            recipe.PositionPoints.Add(CreatePosPoint("PickupPos", "取料位置", 100, (150,80,30)));
            recipe.PositionPoints.Add(CreatePosPoint("DispensePos", "点胶位置", 80, (200,150,15)));
            recipe.PositionPoints.Add(CreatePosPoint("UvCurePos", "UV固化位置", 60, (250,150,25)));
            recipe.PositionPoints.Add(CreatePosPoint("Camera1Pos", "相机1检测位置", 100, (100,200,50)));
            recipe.PositionPoints.Add(CreatePosPoint("PlacePos", "放料位置", 100, (50,50,10)));

            SaveRecipe(recipe);
            _recipes.Add(recipe);

            Logger.Info("BlockCut 标准生产配方已创建: BLOCKCUT-STD (8轴 + 8工艺参数 + 6位置点)");
        }

        private PositionPoint CreatePosPoint(string name, string desc, double velocity, (double x, double y, double z) pos)
        {
            var pt = new PositionPoint { Name = name, Description = desc, Velocity = velocity, IsRelative = false };
            pt.AxisPositions["X"] = pos.x;
            pt.AxisPositions["Y"] = pos.y;
            pt.AxisPositions["Z"] = pos.z;
            return pt;
        }

        /// <summary>
        /// 加载所有配方
        /// </summary>
        private void LoadAllRecipes()
        {
            try
            {
                var files = Directory.GetFiles(_recipeFolder, "*.xml");
                foreach (var file in files)
                {
                    try
                    {
                        var recipe = LoadRecipeFromFile(file);
                        if (recipe != null)
                        {
                            _recipes.Add(recipe);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"加载配方文件失败: {file}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("加载配方列表失败", ex);
            }
        }

        /// <summary>
        /// 从文件加载配方
        /// </summary>
        private RecipeData LoadRecipeFromFile(string filePath)
        {
            var serializer = new XmlSerializer(typeof(RecipeData));
            using (var reader = new StreamReader(filePath))
            {
                return (RecipeData)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// 保存配方到文件
        /// </summary>
        private void SaveRecipeToFile(RecipeData recipe)
        {
            string filePath = Path.Combine(_recipeFolder, $"{recipe.Name}.xml");
            var serializer = new XmlSerializer(typeof(RecipeData));
            using (var writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, recipe);
            }
        }

        /// <summary>
        /// 创建新配方
        /// </summary>
        public RecipeData CreateRecipe(string name, string description = null)
        {
            lock (_lock)
            {
                if (_recipes.Any(r => r.Name == name))
                {
                    Logger.Warning($"配方名称已存在: {name}");
                    return null;
                }

                var recipe = new RecipeData
                {
                    Name = name,
                    Description = description
                };

                _recipes.Add(recipe);
                SaveRecipe(recipe);

                Logger.Info($"创建新配方: {name}");
                return recipe;
            }
        }

        /// <summary>
        /// 添加配方
        /// </summary>
        public void AddRecipe(RecipeData recipe)
        {
            lock (_lock)
            {
                if (recipe == null)
                    return;

                if (string.IsNullOrEmpty(recipe.Id))
                {
                    recipe.Id = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                }

                if (_recipes.Any(r => r.Name == recipe.Name))
                {
                    recipe.Name = $"{recipe.Name}_{DateTime.Now:yyyyMMddHHmmss}";
                }

                _recipes.Add(recipe);
                SaveRecipe(recipe);

                Logger.Info($"添加配方: {recipe.Name}");
            }
        }

        /// <summary>
        /// 保存配方
        /// </summary>
        public bool SaveRecipe(RecipeData recipe)
        {
            lock (_lock)
            {
                try
                {
                    recipe.ModifyTime = DateTime.Now;
                    SaveRecipeToFile(recipe);

                    RecipeSaved?.Invoke(this, recipe);
                    Logger.Info($"保存配方: {recipe.Name}");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error($"保存配方失败: {recipe.Name}", ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// 删除配方
        /// </summary>
        public bool DeleteRecipe(string recipeId)
        {
            lock (_lock)
            {
                var recipe = _recipes.FirstOrDefault(r => r.Id == recipeId);
                if (recipe == null)
                    return false;

                if (recipe.IsDefault)
                {
                    Logger.Warning("不能删除默认配方");
                    return false;
                }

                if (_currentRecipe?.Id == recipeId)
                {
                    Logger.Warning("不能删除当前使用的配方");
                    return false;
                }

                _recipes.Remove(recipe);

                // 删除文件
                string filePath = Path.Combine(_recipeFolder, $"{recipe.Name}.xml");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                RecipeDeleted?.Invoke(this, recipe);
                Logger.Info($"删除配方: {recipe.Name}");
                return true;
            }
        }

        /// <summary>
        /// 加载配方
        /// </summary>
        public bool LoadRecipe(string recipeId)
        {
            lock (_lock)
            {
                var recipe = _recipes.FirstOrDefault(r => r.Id == recipeId);
                if (recipe == null)
                    return false;

                _currentRecipe = recipe;
                RecipeLoaded?.Invoke(this, recipe);
                CurrentRecipeChanged?.Invoke(this, recipe);

                Logger.Info($"加载配方: {recipe.Name}");
                return true;
            }
        }

        /// <summary>
        /// 加载配方（按名称）
        /// </summary>
        public bool LoadRecipeByName(string name)
        {
            lock (_lock)
            {
                var recipe = _recipes.FirstOrDefault(r => r.Name == name);
                if (recipe == null)
                    return false;

                return LoadRecipe(recipe.Id);
            }
        }

        /// <summary>
        /// 复制配方
        /// </summary>
        public RecipeData CopyRecipe(string sourceRecipeId, string newName = null)
        {
            lock (_lock)
            {
                var sourceRecipe = _recipes.FirstOrDefault(r => r.Id == sourceRecipeId);
                if (sourceRecipe == null)
                    return null;

                if (string.IsNullOrEmpty(newName))
                {
                    newName = $"{sourceRecipe.Name}_副本";
                }

                if (_recipes.Any(r => r.Name == newName))
                {
                    newName = $"{newName}_{DateTime.Now:yyyyMMddHHmmss}";
                }

                // 序列化和反序列化实现深拷贝
                var serializer = new XmlSerializer(typeof(RecipeData));
                using (var writer = new StringWriter())
                {
                    serializer.Serialize(writer, sourceRecipe);
                    using (var reader = new StringReader(writer.ToString()))
                    {
                        var newRecipe = (RecipeData)serializer.Deserialize(reader);
                        newRecipe.Id = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                        newRecipe.Name = newName;
                        newRecipe.CreateTime = DateTime.Now;
                        newRecipe.ModifyTime = null;
                        newRecipe.IsDefault = false;

                        _recipes.Add(newRecipe);
                        SaveRecipe(newRecipe);

                        Logger.Info($"复制配方: {sourceRecipe.Name} -> {newName}");
                        return newRecipe;
                    }
                }
            }
        }

        /// <summary>
        /// 获取配方列表
        /// </summary>
        public List<RecipeData> GetRecipeList()
        {
            lock (_lock)
            {
                return _recipes.Select(r => new RecipeData
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    ProductModel = r.ProductModel,
                    Version = r.Version,
                    CreateTime = r.CreateTime,
                    ModifyTime = r.ModifyTime,
                    Author = r.Author,
                    IsDefault = r.IsDefault
                }).ToList();
            }
        }

        /// <summary>
        /// 导出配方
        /// </summary>
        public bool ExportRecipe(string recipeId, string filePath)
        {
            lock (_lock)
            {
                var recipe = _recipes.FirstOrDefault(r => r.Id == recipeId);
                if (recipe == null)
                    return false;

                try
                {
                    var serializer = new XmlSerializer(typeof(RecipeData));
                    using (var writer = new StreamWriter(filePath))
                    {
                        serializer.Serialize(writer, recipe);
                    }

                    Logger.Info($"导出配方: {recipe.Name} -> {filePath}");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error($"导出配方失败: {recipe.Name}", ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// 导入配方
        /// </summary>
        public RecipeData ImportRecipe(string filePath)
        {
            lock (_lock)
            {
                try
                {
                    var recipe = LoadRecipeFromFile(filePath);

                    // 检查是否已存在
                    if (_recipes.Any(r => r.Name == recipe.Name))
                    {
                        recipe.Name = $"{recipe.Name}_{DateTime.Now:yyyyMMddHHmmss}";
                    }

                    recipe.Id = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                    recipe.CreateTime = DateTime.Now;
                    recipe.ModifyTime = null;
                    recipe.IsDefault = false;

                    _recipes.Add(recipe);
                    SaveRecipe(recipe);

                    Logger.Info($"导入配方: {recipe.Name}");
                    return recipe;
                }
                catch (Exception ex)
                {
                    Logger.Error($"导入配方失败: {filePath}", ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _currentRecipe = null;
            _recipes.Clear();
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
