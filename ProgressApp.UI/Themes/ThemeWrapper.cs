using ProgressApp.Core.Interfaces;
using ProgressApp.Core.Models.Enums;
using ProgressApp.WpfUI.Themes.Managers;

namespace ProgressApp.WpfUI.Themes
{
    public class ThemeWrapper : IThemeService
    {
        public void SetTheme(AppTheme theme)
        {
            ThemeManager.ApplyTheme(theme);
        }
    }

}
