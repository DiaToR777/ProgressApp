using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Core.Data;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Heatmap;
using ProgressApp.Core.Models.Journal;
using Serilog;

namespace ProgressApp.Core.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public AnalyticsService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<DateTime?> GetFirstEntryDateAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

                var firstEntry = await context.Entries
                    .AsNoTracking()
                    .OrderBy(e => e.Date)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

                Log.Debug("AnalyticsService: First entry date: {Date}", firstEntry?.Date);
                return firstEntry?.Date;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AnalyticsService: Failed to get first entry date.");
                throw new AppException("Msg_ErrorLoadingHeatmapData", isCritical: true); 
                //TODO Custom ServiceResult 
            }
        }

        public async Task<List<DayCell>> GetHeatmapCells(DateTime from, DateTime to)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

                var entries = await context.Entries
                    .AsNoTracking()
                    .Where(e => e.Date >= from && e.Date <= to)
                    .OrderBy(e => e.Date)
                    .ToDictionaryAsync(e => e.Date.Date, e => e)
                    .ConfigureAwait(false);

                var cells = new List<DayCell>();

                for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
                {
                    var entry = entries.GetValueOrDefault(date);
                    cells.Add(new DayCell
                    {
                        Date = date,
                        Result = entry?.Result,
                        Description = entry?.Description
                    });
                }

                Log.Debug("AnalyticsService: Fetched {Count} heatmap cells from {From} to {To}.",
                    cells.Count, from.ToShortDateString(), to.ToShortDateString());

                return cells;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AnalyticsService: Failed to get heatmap cells from {From} to {To}.", from, to);
                throw new AppException("Msg_ErrorLoadingHeatmapData", isCritical: true); 
            }
        }

        public async Task<int> GetCurrentStreakAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

                var entries = await context.Entries
                    .AsNoTracking()
                    .OrderByDescending(e => e.Date)
                    .Select(e => new { e.Date, e.Result })
                    .ToListAsync()
                    .ConfigureAwait(false);

                var streak = CalculateCurrentStreak(entries.Select(e => (e.Date, e.Result)));
                Log.Debug("AnalyticsService: Current streak: {Streak} days.", streak);
                return streak;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AnalyticsService: Failed to calculate current streak.");
                throw new AppException("Msg_ErrorLoadingStreak", isCritical: true);
            }
        }

        private int CalculateCurrentStreak(IEnumerable<(DateTime Date, DayResult Result)> entries)
        {
            var entriesList = entries.ToList();
            if (!entriesList.Any()) return 0;

            var today = DateTime.Today;

            if (entriesList[0].Date.Date < today.AddDays(-1))
                return 0;

            int streak = 0;
            DateTime expectedDate = entriesList[0].Date.Date;

            foreach (var entry in entriesList)
            {
                if (entry.Result == DayResult.Relapse)
                    break;

                if (entry.Date.Date == expectedDate)
                {
                    streak++;
                    expectedDate = expectedDate.AddDays(-1);
                }
                else if (entry.Date.Date < expectedDate)
                    break;
            }

            return streak;
        }
    }
}