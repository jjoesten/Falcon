using System;
using System.Windows.Markup;

namespace Aesalon
{
    public class EnumBindingSourceExtension : MarkupExtension
    {
        private Type enumType;
        public Type EnumType
        {
            get { return enumType; }
            set
            {
                if (value != enumType)
                {
                    if (value != null)
                    {
                        Type et = Nullable.GetUnderlyingType(value) ?? value;
                        if (!et.IsEnum)
                            throw new ArgumentException("Type must be an Enum");
                    }

                    enumType = value;
                }
            }
        }

        public EnumBindingSourceExtension() { }
        public EnumBindingSourceExtension(Type enumType)
        {
            this.enumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (enumType == null)
                throw new InvalidOperationException("The type of Enum must be specific.");

            Type actualEnumType = Nullable.GetUnderlyingType(enumType) ?? enumType;
            Array enumValues = Enum.GetValues(actualEnumType);

            if (actualEnumType == enumType)
                return enumValues;

            Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }
    }
}
