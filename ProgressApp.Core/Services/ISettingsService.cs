using ProgressApp.Core.Models.Enums;
using ProgressApp.Core.Models.Localization;

namespace ProgressApp.Core.Services
{
    public interface ISettingsService
    {
        bool IsFirstRun();
        string GetGoal();
        string GetUserName();
        AppTheme GetTheme();
        LanguageModel GetLanguage();
        void SaveSettings(string username, string goal, AppTheme theme, LanguageModel language);

    }
}
