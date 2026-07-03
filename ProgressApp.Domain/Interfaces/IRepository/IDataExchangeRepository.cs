using ProgressApp.Domain.Models.Journal;

namespace ProgressApp.Domain.Interfaces.IRepository;

public interface IDataExchangeRepository //todo
{
    Task<List<DailyCheckin>> GetAllEntriesAsync(); 
    Task<string?> GetGoalSettingValueAsync();
    Task ReplaceDataAsync(List<DailyCheckin> entries, string? goalValue);
}
