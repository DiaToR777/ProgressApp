using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Core.Data;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
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

        public async Task SaveGoalAsync(string goal)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

                if (string.IsNullOrWhiteSpace(goal)) throw new AppException("Msg_GoalEmpty");
                await UpdateOrAddAsync(context, SettingsKeys.Goal, goal);
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
