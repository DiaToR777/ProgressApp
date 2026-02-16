using System.Globalization;
using System.Windows.Data;

namespace ProgressApp.Localization.Helpers
{
    public class EnumToLocalizationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                var type = enumValue.GetType();
                var genericType = typeof(LocalizedEnum<>).MakeGenericType(type);
                var instance = Activator.CreateInstance(genericType, enumValue);

                return genericType.GetProperty("DisplayName")?.GetValue(instance) ?? value.ToString();
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
