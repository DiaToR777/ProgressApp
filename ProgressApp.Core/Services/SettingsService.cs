using Microsoft.EntityFrameworkCore;
using ProgressApp.Core.Data;
using ProgressApp.Core.Models.Enums;
using ProgressApp.Core.Models.Localization;
using ProgressApp.Core.Models.Settings;
using Serilog;

namespace ProgressApp.Core.Services
{
    public class SettingsService : ISettingsService
    {
        private ProgressDbContext _context;
        public SettingsService(ProgressDbContext context)
        {
            _context = context;
        }

        public bool IsFirstRun()
        {
            var usernameSetting = _context.Settings
                    .FirstOrDefault(s => s.Key == SettingsKeys.Username);

            // Якщо запису немає (раптом автоматика збійнула) 
            // АБО якщо він є, але значення порожнє — значить юзер ще не проходив Setup.
            bool isFirst = usernameSetting == null || string.IsNullOrWhiteSpace(usernameSetting.Value);

            Log.Debug("FirstRun check: Username value is '{Value}'. IsFirstRun: {Result}",
                usernameSetting?.Value ?? "NULL", isFirst);

            return isFirst;
        }

        public string GetUserName() => GetValue(SettingsKeys.Username) ?? "";
        public string GetGoal() => GetValue(SettingsKeys.Goal) ?? "";

        private string? GetValue(string key)
        {
            try
            {
                return _context.Settings.AsNoTracking().FirstOrDefault(s => s.Key == key)?.Value;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load setting for key: {Key}", key);
                return null;
            }
        }

        public AppTheme GetTheme()
        {
            var themeValue = GetValue(SettingsKeys.Theme);
            if (Enum.TryParse(themeValue, out AppTheme result)) return result;

            Log.Warning("Theme not found or corrupted in DB. Falling back to Light.");
            return AppTheme.Light;
        }

        public LanguageModel GetLanguage()
        {
            string? code = GetValue(SettingsKeys.Language);
            return LanguageConfig.GetByCode(code);
        }

        public void SaveSettings(string username, string goal, AppTheme theme, LanguageModel language)
        {
            try
            {
                Log.Information("Saving application settings for user: {Username}", username);

                if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be empty");
                if (string.IsNullOrWhiteSpace(goal)) throw new ArgumentException("Goal cannot be empty");

                UpdateOrAdd(SettingsKeys.Username, username);
                UpdateOrAdd(SettingsKeys.Goal, goal);
                UpdateOrAdd(SettingsKeys.Theme, theme.ToString());
                UpdateOrAdd(SettingsKeys.Language, language.CultureCode);

                _context.SaveChanges();
                Log.Information("All settings saved successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fatal error while saving settings!");
                throw;
            }
        }

        private void UpdateOrAdd(string key, string value)
        {
            var setting = _context.Settings.FirstOrDefault(s => s.Key == key);
            if (setting != null)
            {
                setting.Value = value;
            }
            else
            {
                Log.Debug("Creating new setting entry: {Key} = {Value}", key, value);
                _context.Settings.Add(new AppSettings { Key = key, Value = value });
            }
        }
    }

}
