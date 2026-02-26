using ProgressApp.Core.Models.Journal;

namespace ProgressApp.Core.Services
{
    public interface IJournalService
    {
         JournalEntry? GetToday();
         void SaveToday(string description, DayResult result);
         List<JournalEntry> GetAllEntries();

    }
}
