using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Models.Settings;
using ProgressApp.Infrastructure.Data;

namespace ProgressApp.Infrastructure.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SettingsRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<string> GetGoalAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
            return await GetValueAsync(context, SettingsKeys.Goal) ?? "";
        }

        public async Task SaveGoalAsync(string goal)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            await UpdateOrAddAsync(context, SettingsKeys.Goal, goal);
            await context.SaveChangesAsync();
        }

        private async Task<string?> GetValueAsync(ProgressDbContext context, string key)
        {
            var setting = await context.Settings
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Key == key);

            return setting?.Value;
        }

        private async Task UpdateOrAddAsync(ProgressDbContext context, string key, string value)
        {
            var setting = await context.Settings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (setting != null)
                setting.Value = value;
            else
                context.Settings.Add(new AppSettings { Key = key, Value = value });
        }
    }
}
