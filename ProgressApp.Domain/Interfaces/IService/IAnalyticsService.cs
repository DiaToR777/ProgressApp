using ProgressApp.Domain.Models.Goals;
using ProgressApp.Domain.Models.Heatmap;

namespace ProgressApp.Domain.Interfaces.IService;

public interface IAnalyticsService
{
    Task<MilestoneProgress?> GetActiveMilestoneProgressAsync();
    Task<int> GetCurrentStreakAsync();
    Task<List<DayCell>> GetHeatmapCellsAsync(DateTime from, DateTime to);
    Task<DateTime?> GetFirstEntryDateAsync();
}