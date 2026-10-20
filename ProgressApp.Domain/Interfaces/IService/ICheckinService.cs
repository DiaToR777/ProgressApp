using ProgressApp.Domain.Models.Goals;
using ProgressApp.Domain.Models.Journal;

namespace ProgressApp.Domain.Interfaces.IService;

public interface ICheckinService
{
    Task<DailyCheckin?> GetTodayAsync();
    Task<List<GoalAction>> GetActiveActionsAsync();
    Task SaveTodayAsync(string description, DayResult result, Dictionary<Guid, bool> actionCompletions);
    Task<List<DailyCheckin>> GetAllEntriesAsync();
    Task<MilestoneProgress?> GetActiveMilestoneProgressAsync();
}
