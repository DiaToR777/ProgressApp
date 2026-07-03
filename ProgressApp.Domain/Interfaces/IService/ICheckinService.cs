using ProgressApp.Domain.Models.Journal;

namespace ProgressApp.Domain.Interfaces.IService;

public interface ICheckinService
{
    Task<DailyCheckin?> GetTodayAsync();
    Task SaveTodayAsync(string description, DayResult result);
    Task<List<DailyCheckin>> GetAllEntriesAsync();

}
