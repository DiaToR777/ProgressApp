using ProgressApp.Models.Journal;

namespace ProgressApp.Services
{
    public interface IJournalService
    {
         JournalEntry? GetToday();
         void SaveToday(string description, DayResult result);
         List<JournalEntry> GetAllEntries();

    }
}
