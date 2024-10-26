using System;
using System.Windows.Markup;

namespace Apps.Devlosys.Infrastructure.Extensions
{
    public class EnumBindingSourceExtensions : MarkupExtension
    {
        public Type EnumType { get; private set; }

        public EnumBindingSourceExtensions(Type EnumType)
        {
            if (EnumType is null || !EnumType.IsEnum)
                throw new Exception("EnumType must not be null and of type Enum.");

            this.EnumType = EnumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(EnumType);
        }
    }
}
