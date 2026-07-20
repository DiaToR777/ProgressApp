using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProgressApp.WpfUI.Converters.Heatmap;

public class ScoreToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double score)
            return new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)); 
        
        return score switch
        {
            >= 0.66 => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)), 
            >= 0.33 => new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07)), 
            _ => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36))        
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}