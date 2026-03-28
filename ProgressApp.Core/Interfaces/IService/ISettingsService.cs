using ProgressApp.Core.Models.Enums;
using ProgressApp.Core.Models.Localization;

namespace ProgressApp.Core.Interfaces.IService
{
    public interface ISettingsService
    {
        Task<string> GetGoalAsync();
        Task<string> GetUserNameAsync();
        Task<AppTheme> GetThemeAsync();
        Task<LanguageModel> GetLanguageAsync();
        Task SaveSettingsAsync(string username, string goal, AppTheme theme, LanguageModel language);

    }
}
