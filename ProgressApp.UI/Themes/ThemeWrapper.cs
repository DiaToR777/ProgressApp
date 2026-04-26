using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Enums;
using ProgressApp.WpfUI.Themes.Managers;

namespace ProgressApp.WpfUI.Themes
{
    public class ThemeWrapper : IAppThemeService
    {
        public void SetTheme(AppTheme theme)
        {
            ThemeManager.ApplyTheme(theme);
        }
    }
}
