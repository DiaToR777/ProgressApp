using ProgressApp.Localization.Models;

namespace ProgressApp.Localization
{
    public static class LanguageConfig
    {
        public static List<LanguageModel> AvailableLanguages { get; } = new()
    {
        new LanguageModel { Name = "Українська", CultureCode = "uk-UA" },
        new LanguageModel { Name = "English", CultureCode = "en-US" }
    };
    }
}

