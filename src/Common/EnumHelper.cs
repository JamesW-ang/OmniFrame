using System;
using System.ComponentModel;
using System.Reflection;

namespace OmniFrame.Common
{
    public static class EnumHelper
    {
        public static string GetDescription(Enum value)
        {
            if (value == null)
                return string.Empty;

            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field == null)
                return value.ToString();

            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        public static T Parse<T>(string value, T defaultValue = default) where T : struct
        {
            if (Enum.TryParse<T>(value, out T result))
                return result;
            return defaultValue;
        }

        public static T Parse<T>(string value, bool ignoreCase, T defaultValue = default) where T : struct
        {
            if (Enum.TryParse<T>(value, ignoreCase, out T result))
                return result;
            return defaultValue;
        }
    }
}
