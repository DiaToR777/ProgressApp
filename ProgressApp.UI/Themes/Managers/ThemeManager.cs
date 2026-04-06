using ProgressApp.Core.Models.Enums;
using Wpf.Ui.Appearance;

namespace ProgressApp.WpfUI.Themes.Managers;

public static class ThemeManager
{
    public static void ApplyTheme(AppTheme theme)
    {
        var uiTheme = theme == AppTheme.Dark
                    ? ApplicationTheme.Dark
                    : ApplicationTheme.Light;
        ApplicationThemeManager.Apply(uiTheme);
    }
}