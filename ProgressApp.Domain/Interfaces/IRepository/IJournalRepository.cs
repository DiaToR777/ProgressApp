using ProgressApp.Domain.Models.Journal;

namespace ProgressApp.Domain.Interfaces.IRepository
{
    public interface IJournalRepository
    {
        Task<JournalEntry?> GetTodayAsync();
        Task SaveTodayAsync(string description, DayResult result);
        Task<List<JournalEntry>> GetAllEntriesAsync();

    }
}
