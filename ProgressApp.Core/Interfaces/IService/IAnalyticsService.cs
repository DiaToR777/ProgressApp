
using ProgressApp.Core.Models.Heatmap;

namespace ProgressApp.Core.Interfaces.IService
{
    public interface IAnalyticsService
    {
        Task<int> GetCurrentStreakAsync();
        Task<List<DayCell>> GetHeatmapCells(DateTime from, DateTime to);
        Task<List<DayCell>> GetAllHeatmapCellsAsync();
        Task<DateTime?> GetFirstEntryDateAsync();
    }
}
