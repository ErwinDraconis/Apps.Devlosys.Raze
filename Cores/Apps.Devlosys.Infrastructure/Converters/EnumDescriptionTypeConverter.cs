using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Apps.Devlosys.Infrastructure.Converters
{
    public class EnumDescriptionTypeConverter : EnumConverter
    {
        public EnumDescriptionTypeConverter(Type type) : base(type) { }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                FieldInfo fieldInfo = value?.GetType().GetField(value.ToString());
                DescriptionAttribute attribute = (DescriptionAttribute)fieldInfo?.GetCustomAttribute(typeof(DescriptionAttribute), false);

                return attribute?.Description;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
