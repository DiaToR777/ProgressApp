using ProgressApp.Localization.Models;

namespace ProgressApp.Services
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
