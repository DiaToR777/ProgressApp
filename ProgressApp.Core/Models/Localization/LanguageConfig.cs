namespace ProgressApp.Core.Models.Localization
{
    public static class LanguageConfig
    {
        public static List<LanguageModel> AvailableLanguages { get; } = new()
    {
        new LanguageModel { Name = "English", CultureCode = "en-US" },
        new LanguageModel { Name = "Українська", CultureCode = "uk-UA" }
    };

        public static LanguageModel GetByCode(string? code)
        {
            return AvailableLanguages.FirstOrDefault(l => l.CultureCode == code)
                   ?? AvailableLanguages.First(); 
        }
    }
}

