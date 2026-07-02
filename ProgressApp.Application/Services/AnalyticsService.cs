using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Models.Heatmap;
using ProgressApp.Domain.Models.Journal;
using Serilog;

namespace ProgressApp.Application.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _analyticsRepository;
        public AnalyticsService(IAnalyticsRepository analyticsRepository)
        {
            _analyticsRepository = analyticsRepository;
        }

        public async Task<DateTime?> GetFirstEntryDateAsync()
        {
            try
            {
                var firstEntry = await _analyticsRepository.GetFirstEntryDateAsync();

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
                var entries = await _analyticsRepository.GetEntriesByDateRangeAsync(from, to);
                var lookup = entries.ToDictionary(e => e.Date.Date);

                var cells = new List<DayCell>();
                for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
                {
                    var entry = lookup.GetValueOrDefault(date);
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
                var entries = await _analyticsRepository.GetEntriesForStreakAsync();

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