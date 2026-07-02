using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Models.Journal;
using ProgressApp.Domain.Models.Settings;
using ProgressApp.Infrastructure.Data;

namespace ProgressApp.Infrastructure.Repositories
{
    public class DataExchangeRepository : IDataExchangeRepository
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DataExchangeRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            return await context.Entries.AsNoTracking().ToListAsync();
        }

        public async Task<string?> GetGoalSettingValueAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            var goal = await context.Settings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Key == SettingsKeys.Goal);
            return goal?.Value;
        }

        public async Task ReplaceDataAsync(List<JournalEntry> entries, string? goalValue)
        {

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                entries.ForEach(e => e.Date = e.Date.Date);

                await context.Entries.ExecuteDeleteAsync();
                await context.Entries.AddRangeAsync(entries);

                if (goalValue != null)
                {
                    var setting = await context.Settings.FirstOrDefaultAsync(s => s.Key == SettingsKeys.Goal);
                    if (setting != null)
                        setting.Value = goalValue;
                    else
                        context.Settings.Add(new AppSettings { Key = SettingsKeys.Goal, Value = goalValue });
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; 
            }
        }
    }
}
