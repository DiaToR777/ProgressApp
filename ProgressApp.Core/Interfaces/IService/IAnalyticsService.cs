
using ProgressApp.Core.Models.Heatmap;

namespace ProgressApp.Core.Interfaces.IService
{
    public interface IAnalyticsService
    {
        Task<int> GetCurrentStreakAsync();
        Task<List<DayCell>> GetHeatmapCells(DateTime from, DateTime to);
        Task<DateTime?> GetFirstEntryDateAsync();
    }
}
