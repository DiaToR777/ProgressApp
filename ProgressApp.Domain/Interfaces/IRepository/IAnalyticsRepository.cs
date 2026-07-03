using ProgressApp.Domain.Models.Journal;

namespace ProgressApp.Domain.Interfaces.IRepository;

public interface IAnalyticsRepository
{
    Task<List<DailyCheckin>> GetEntriesForStreakAsync();
    Task<List<DailyCheckin>> GetEntriesByDateRangeAsync(DateTime from, DateTime to);
    Task<DateTime?> GetFirstEntryDateAsync();

}
