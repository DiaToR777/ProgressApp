using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Core.Data;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Journal;
using Serilog;

namespace ProgressApp.Core.Services
{
    public class JournalService : IJournalService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public JournalService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        private async Task<JournalEntry?> GetTodayInternalAsync(ProgressDbContext context)
        {
            var today = DateTime.Today;
            return await context.Entries
                .FirstOrDefaultAsync(e => e.Date.Date == today)
                .ConfigureAwait(false);
        }

        public async Task<JournalEntry?> GetTodayAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

                return await GetTodayInternalAsync(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while checking for today's entry in database.");
                throw new AppException("Msg_ErrorLoadingData");
            }
        }

        public async Task SaveTodayAsync(string description, DayResult result)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

                if (string.IsNullOrWhiteSpace(description))
                {
                    Log.Warning("Attempted to save today's entry with empty description.");
                    throw new AppException("Msg_DescriptionEmpty");
                }

                var entry = await GetTodayInternalAsync(context);
                bool isNew = entry == null;

                if (isNew)
                {
                    entry = new JournalEntry
                    {
                        Date = DateTime.Today,
                        CreatedAt = DateTime.Now
                    };
                    await context.Entries.AddAsync(entry).ConfigureAwait(false);
                }

                entry.Description = description;
                entry.Result = result;

                if (isNew)
                    Log.Information("Creating new entry: Result: {Result}", result);
                else
                    Log.Information("Updating entry: Result:{Result}", result);

                await context.SaveChangesAsync();

                Log.Information("Entry {Action} successfully. ID: {Id}", isNew ? "created" : "updated", entry.Id);
            }
            catch (Exception ex) when (ex is not AppException)
            {

                Log.Error(ex, "Error occurred while saving today's entry");
                throw new AppException("Msg_SaveEntryError");
            }
        }

        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

                var entries = await context.Entries
                            .AsNoTracking()
                            .OrderByDescending(e => e.Date)
                            .ToListAsync()
                            .ConfigureAwait(false);

                Log.Debug("Fetched {Count} entries from database.", entries.Count);
                return entries;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch all journal entries.");
                throw new AppException("Msg_ErrorLoadingData");
            }
        }

        public async Task<int> GetCurrentStreakAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            var dates = await context.Entries
                            .Where(e => e.Result == DayResult.Success || e.Result == DayResult.PartialSuccess)
                            .Select(e => e.Date)
                            .OrderByDescending(d => d)
                            .ToListAsync();


            return CalculateStreak(dates);
        }

        private int CalculateStreak(List<DateTime> dates)
        {
            if (!dates.Any()) return 0;

            var today = DateTime.Today;
            var lastEntryDate = dates[0].Date;

            if (lastEntryDate < today.AddDays(-1))
                return 0;

            int streak = 1;
            var currentCompare = lastEntryDate;

            for (int i = 1; i < dates.Count; i++)
            {
                var nextDate = dates[i].Date;
                if (nextDate == currentCompare) continue;
                if (nextDate == currentCompare.AddDays(-1))
                {
                    streak++;
                    currentCompare = nextDate;
                }
                else
                {
                    break;
                }
            }

            return streak;
        }
                
    }
}
