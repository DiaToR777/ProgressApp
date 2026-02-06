using System.Windows;

namespace ProgressApp.Themes;
public static class ThemeManager
{
    public static void ApplyTheme(AppTheme theme)
    {
        var themeName = theme == AppTheme.Dark ? "DarkTheme" : "LightTheme";
        var uri = new Uri($"Themes/{themeName}.xaml", UriKind.Relative);

        ResourceDictionary newDict = new ResourceDictionary { Source = uri };

        var appRes = Application.Current.Resources.MergedDictionaries;
        var oldTheme = appRes.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme"));

        if (oldTheme != null)
        {
            appRes.Remove(oldTheme);
        }
        appRes.Add(newDict);
    }
}
