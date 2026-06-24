using ProgressApp.Domain.Models.Journal;

namespace ProgressApp.Domain.Interfaces.IRepository;

public interface IDataExchangeRepository
{
    Task<List<JournalEntry>> GetAllEntriesAsync();
    Task<string?> GetGoalSettingValueAsync();
    Task ReplaceDataAsync(List<JournalEntry> entries, string? goalValue);
}
