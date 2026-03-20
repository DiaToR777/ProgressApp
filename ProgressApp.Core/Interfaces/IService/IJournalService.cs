using ProgressApp.Core.Models.Journal;

namespace ProgressApp.Core.Interfaces.IService
{
    public interface IJournalService
    {
         JournalEntry? GetToday();
         void SaveToday(string description, DayResult result);
         List<JournalEntry> GetAllEntries();

    }
}
