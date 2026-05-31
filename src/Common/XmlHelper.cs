using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace OmniFrame.Common
{
    public static class XmlHelper
    {
        public static string GetAttributeValue(XmlNode node, string attrName, string defaultValue = "")
        {
            if (node == null || node.Attributes == null)
                return defaultValue;

            XmlAttribute attr = node.Attributes[attrName];
            return attr?.Value ?? defaultValue;
        }

        public static int GetAttributeValueInt(XmlNode node, string attrName, int defaultValue = 0)
        {
            string value = GetAttributeValue(node, attrName);
            if (int.TryParse(value, out int result))
                return result;
            return defaultValue;
        }

        public static double GetAttributeValueDouble(XmlNode node, string attrName, double defaultValue = 0)
        {
            string value = GetAttributeValue(node, attrName);
            if (double.TryParse(value, out double result))
                return result;
            return defaultValue;
        }

        public static bool GetAttributeValueBool(XmlNode node, string attrName, bool defaultValue = false)
        {
            string value = GetAttributeValue(node, attrName).ToLower();
            if (value == "true" || value == "1" || value == "yes")
                return true;
            if (value == "false" || value == "0" || value == "no")
                return false;
            return defaultValue;
        }

        public static void SetAttributeValue(XmlNode node, string attrName, string value)
        {
            if (node == null)
                return;

            XmlAttribute attr = node.Attributes[attrName];
            if (attr == null)
            {
                attr = node.OwnerDocument.CreateAttribute(attrName);
                node.Attributes.Append(attr);
            }
            attr.Value = value;
        }

        public static XmlNode CreateNode(XmlDocument doc, string nodeName, Dictionary<string, string> attributes = null)
        {
            XmlNode node = doc.CreateElement(nodeName);
            if (attributes != null)
            {
                foreach (var attr in attributes)
                {
                    SetAttributeValue(node, attr.Key, attr.Value);
                }
            }
            return node;
        }

        public static T Deserialize<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        public static string Serialize<T>(T obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }
    }
}
