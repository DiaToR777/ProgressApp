using ProgressApp.Core.Models.Journal;


namespace ProgressApp.Core.Interfaces.IService
{
    public interface IJournalService
    {
        Task<JournalEntry?> GetTodayAsync();
        Task SaveTodayAsync(string description, DayResult result);
        Task<List<JournalEntry>> GetAllEntriesAsync();

    }
}
