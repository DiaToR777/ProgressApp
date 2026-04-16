using ProgressApp.Core.Models.Journal;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProgressApp.WpfUI.Converters.Heatmap;

public class DayResultToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is DayResult result ? result switch
        {
            DayResult.Success => new SolidColorBrush(Color.FromRgb(99, 153, 34)),  // green
            DayResult.Relapse => new SolidColorBrush(Color.FromRgb(163, 45, 45)), // red
            DayResult.PartialSuccess => new SolidColorBrush(Color.FromRgb(186, 117, 23)), // amber
            _ => new SolidColorBrush(Color.FromRgb(200, 200, 200))
        } : Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}