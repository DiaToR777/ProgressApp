using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceScopeFactory _scopeFactory;
        public SettingsService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<string> GetUserNameAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
            return await GetValueAsync(context, SettingsKeys.Username) ?? "";

        }
        public async Task<string> GetGoalAsync()
        {

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
            return await GetValueAsync(context, SettingsKeys.Goal) ?? "";
        }

        private async Task<string?> GetValueAsync(ProgressDbContext context, string key)
        {
            try
            {
                var setting = await context.Settings
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
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

                var themeValue = await GetValueAsync(context, SettingsKeys.Theme);
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
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            string? code = await GetValueAsync(context, SettingsKeys.Language);
            return LanguageConfig.GetByCode(code);
        }

        public async Task SaveSettingsAsync(string username, string goal, AppTheme theme, LanguageModel language)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();


                Log.Information("Saving application settings for user: {Username}", username);

                if (string.IsNullOrWhiteSpace(username)) throw new AppException("Msg_UsernameEmpty");
                if (string.IsNullOrWhiteSpace(goal)) throw new AppException("Msg_GoalEmpty");

                await UpdateOrAddAsync(context, SettingsKeys.Username, username);
                await UpdateOrAddAsync(context, SettingsKeys.Goal, goal);
                await UpdateOrAddAsync(context, SettingsKeys.Theme, theme.ToString());
                await UpdateOrAddAsync(context, SettingsKeys.Language, language.CultureCode);

                await context.SaveChangesAsync().ConfigureAwait(false);
                Log.Information("All settings saved successfully.");
            }
            catch (Exception ex) when (ex is not AppException)
            {
                Log.Error(ex, "Fatal error while saving settings!");
                throw new AppException("Msg_SaveSettingsError");
            }
        }

        private async Task UpdateOrAddAsync(ProgressDbContext context, string key, string value)
        {
            var setting = await context.Settings
                .FirstOrDefaultAsync(s => s.Key == key)
                .ConfigureAwait(false);

            if (setting != null)
            {
                setting.Value = value;
            }
            else
            {
                Log.Debug("Creating new setting entry: {Key} = {Value}", key, value);
                context.Settings.Add(new AppSettings { Key = key, Value = value });
            }
        }
    }

}
