using System.ComponentModel;

namespace System
{
    public static class EnumExtensions
    {
        public static string GetDescription<T>(this T enumValue) where T : Enum, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                return null;
            }

            string description = enumValue.ToString();
            Reflection.FieldInfo fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                object[] attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    description = ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return description;
        }

        public static string GetEnum<T>(this T enumValue) where T : Enum, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                return null;
            }

            string description = enumValue.ToString();

            return description;
        }
    }
}
