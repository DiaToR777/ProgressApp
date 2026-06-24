using ProgressApp.Domain.Models.Journal;

namespace ProgressApp.Domain.Interfaces.IService;

public interface IJournalService
{
    Task<JournalEntry?> GetTodayAsync();
    Task SaveTodayAsync(string description, DayResult result);
    Task<List<JournalEntry>> GetAllEntriesAsync();

}
