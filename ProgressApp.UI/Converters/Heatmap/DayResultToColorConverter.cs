using ProgressApp.Core.Models.Journal;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProgressApp.WpfUI.Converters.Heatmap;

public class DayResultToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DayResult result)
            return new SolidColorBrush(Color.FromRgb(180, 180, 180)); // серый для null

        return result switch
        {
            DayResult.Success => new SolidColorBrush(Color.FromRgb(99, 153, 34)),
            DayResult.Relapse => new SolidColorBrush(Color.FromRgb(163, 45, 45)),
            DayResult.PartialSuccess => new SolidColorBrush(Color.FromRgb(186, 117, 23)),
            _ => new SolidColorBrush(Color.FromRgb(180, 180, 180))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}