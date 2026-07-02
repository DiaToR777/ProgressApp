using ProgressApp.Domain.Models.Heatmap;

namespace ProgressApp.Domain.Interfaces.IService;

public interface IAnalyticsService
{
    Task<int> GetCurrentStreakAsync();
    Task<List<DayCell>> GetHeatmapCells(DateTime from, DateTime to);
    Task<DateTime?> GetFirstEntryDateAsync();
}
