using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Core.Data;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Heatmap;
using ProgressApp.Core.Models.Journal;

namespace ProgressApp.Core.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public AnalyticsService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        //add logging and error handling
        public async Task<DateTime?> GetFirstEntryDateAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
            var firstEntry = await context.Entries
                .OrderBy(e => e.Date)
                .FirstOrDefaultAsync();
            return firstEntry?.Date;
        }

        public async Task<List<DayCell>> GetAllHeatmapCellsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            return await context.Entries
                .OrderBy(e => e.Date)
                .Select(e => new DayCell
                {
                    Date = e.Date,
                    Result = e.Result,
                    Description = e.Description
                })
                .ToListAsync();
        }

        public async Task<List<DayCell>> GetHeatmapCells(DateTime from, DateTime to)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            var entries = await context.Entries
                .Where(e => e.Date >= from && e.Date <= to)
                .OrderBy(e => e.Date)
                .ToDictionaryAsync(e => e.Date.Date, e => e);

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

            return cells;
        }

        public async Task<int> GetCurrentStreakAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            var entries = await context.Entries
                                .OrderByDescending(e => e.Date)
                                .Select(e => new { e.Date, e.Result })
                                .ToListAsync();

            return CalculateCurrentStreak(entries.Select(e => (e.Date, e.Result)));
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
