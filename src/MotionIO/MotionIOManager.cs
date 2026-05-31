using System;
using System.Collections.Generic;
using System.Xml;
using OmniFrame.Common;

namespace MotionIO
{
    public interface IMotionIOManager
    {
        void AddMotion(Motion motion);
        Motion GetMotion(int cardIndex);
        Motion GetMotion(string name);
        bool RemoveMotion(int cardIndex);
        void Clear();
        bool InitAll();
        bool DeInitAll();
        void ReadCfgFromXml(XmlDocument doc);
        List<Motion> GetAllMotions();
    }

    public class MotionIOManager : IMotionIOManager
    {
        private List<Motion> _motions = new List<Motion>();


        public MotionIOManager()
        {
        }

        public void AddMotion(Motion motion)
        {
            if (motion != null && !_motions.Contains(motion))
            {
                _motions.Add(motion);
            }
        }

        public Motion GetMotion(int cardIndex)
        {
            foreach (var motion in _motions)
            {
                if (motion.CardIndex == cardIndex)
                    return motion;
            }
            return null;
        }

        public Motion GetMotion(string name)
        {
            foreach (var motion in _motions)
            {
                if (motion.Name == name)
                    return motion;
            }
            return null;
        }

        public bool RemoveMotion(int cardIndex)
        {
            var motion = GetMotion(cardIndex);
            if (motion != null)
            {
                motion.DeInit();
                return _motions.Remove(motion);
            }
            return false;
        }

        public void Clear()
        {
            foreach (var motion in _motions)
            {
                motion.DeInit();
            }
            _motions.Clear();
        }

        public bool InitAll()
        {
            bool result = true;
            foreach (var motion in _motions)
            {
                if (motion.Enable)
                {
                    if (!motion.Init())
                    {
                        result = false;
                        Logger.Error($"初始化运动卡失败: {motion.Name}");
                    }
                }
            }
            return result;
        }

        public bool DeInitAll()
        {
            bool result = true;
            foreach (var motion in _motions)
            {
                if (!motion.DeInit())
                {
                    result = false;
                }
            }
            return result;
        }

        public void ReadCfgFromXml(XmlDocument doc)
        {
            try
            {
                XmlNodeList nodes = doc.SelectNodes("//MotionCard");
                if (nodes == null) return;

                foreach (XmlNode node in nodes)
                {
                    int index = XmlHelper.GetAttributeValueInt(node, "Index", -1);
                    string name = XmlHelper.GetAttributeValue(node, "Name", $"Motion_{index}");
                    string type = XmlHelper.GetAttributeValue(node, "Type", "GTS");
                    int minAxis = XmlHelper.GetAttributeValueInt(node, "MinAxis", 0);
                    int maxAxis = XmlHelper.GetAttributeValueInt(node, "MaxAxis", 7);
                    bool enable = XmlHelper.GetAttributeValueBool(node, "Enable", false);

                    if (index >= 0)
                    {
                        Motion motion = CreateMotion(type, index, name, minAxis, maxAxis);
                        if (motion != null)
                        {
                            motion.SetEnable(enable);
                            AddMotion(motion);
                            Logger.Info($"加载运动卡配置: {name} ({type}, Axis {minAxis}-{maxAxis})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("读取运动卡配置失败", ex);
            }
        }

        private Motion CreateMotion(string type, int index, string name, int minAxis, int maxAxis)
        {
            switch (type.ToUpper())
            {
                case "GTS":
                    return new Motion_GTS(index, name, minAxis, maxAxis);
                case "DMC3000":
                    return new Motion_Dmc3000(index, name, minAxis, maxAxis);
                case "DMC3400":
                    return new Motion_DMC3400(index, name, minAxis, maxAxis);
                case "INOVANCE":
                    return new Motion_InoEcat(index, name, minAxis, maxAxis);
                default:
                    Logger.Error($"未知的运动卡类型: {type}");
                    return null;
            }
        }

        public List<Motion> GetAllMotions()
        {
            return new List<Motion>(_motions);
        }
    }
}
