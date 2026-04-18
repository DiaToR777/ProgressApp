using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Core.Data;
using ProgressApp.Core.Exceptions;
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
                .ToDictionaryAsync(e => e.Date.Date, e => e); // берём весь JournalEntry

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
