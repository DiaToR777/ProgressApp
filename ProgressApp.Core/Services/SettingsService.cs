using Microsoft.EntityFrameworkCore;
using ProgressApp.Core.Data;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
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
            
        public async Task<bool> IsFirstRunAsync()
        {
            var usernameSetting = await _context.Settings
                .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Key == SettingsKeys.Username)
                    .ConfigureAwait(false);

            bool isFirst = usernameSetting == null || string.IsNullOrWhiteSpace(usernameSetting.Value);

            Log.Debug("FirstRun check: Username value is '{Value}'. IsFirstRun: {Result}",
                usernameSetting?.Value ?? "NULL", isFirst);

            return isFirst;
        }

        public async Task<string> GetUserNameAsync() => await GetValueAsync(SettingsKeys.Username) ?? "";
        public async Task<string> GetGoalAsync() => await GetValueAsync(SettingsKeys.Goal) ?? "";

        private async Task<string?> GetValueAsync(string key)
        {
            try
            {
                var setting = await _context.Settings
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.Key == key)
                            .ConfigureAwait(false);
                return setting?.Value;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load setting for key: {Key}", key);
                throw new AppException("Msg_ErrorLoadingSetting", key);
            }
        }

        public async Task<AppTheme> GetThemeAsync()
        {
            try
            {
                var themeValue = await GetValueAsync(SettingsKeys.Theme);
                if (Enum.TryParse(themeValue, out AppTheme result)) return result;

                Log.Warning("Theme not found or corrupted in DB. Falling back to Light.");
                return AppTheme.Light;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to read Theme from database!");
                throw new AppException("Msg_ThemeReadError");
            }
        }

        public async Task<LanguageModel> GetLanguageAsync()
        {
            string? code = await GetValueAsync(SettingsKeys.Language);
            return LanguageConfig.GetByCode(code);
        }

        public async Task SaveSettingsAsync(string username, string goal, AppTheme theme, LanguageModel language)
        {
            try
            {
                Log.Information("Saving application settings for user: {Username}", username);

                if (string.IsNullOrWhiteSpace(username)) throw new AppException("Msg_UsernameEmpty");
                if (string.IsNullOrWhiteSpace(goal)) throw new AppException("Msg_GoalEmpty");

                await UpdateOrAddAsync(SettingsKeys.Username, username);
                await UpdateOrAddAsync(SettingsKeys.Goal, goal);
                await UpdateOrAddAsync(SettingsKeys.Theme, theme.ToString());
                await UpdateOrAddAsync(SettingsKeys.Language, language.CultureCode);

                await _context.SaveChangesAsync().ConfigureAwait(false);
                Log.Information("All settings saved successfully.");
            }
            catch (Exception ex) when (ex is not AppException)
            {
                Log.Error(ex, "Fatal error while saving settings!");
                throw new AppException("Msg_SaveSettingsError");
            }
        }

        private async Task UpdateOrAddAsync(string key, string value)
        {

            var setting = await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == key)
                .ConfigureAwait(false);

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
