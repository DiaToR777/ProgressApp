using ProgressApp.Domain.Models.Goals;

namespace ProgressApp.Domain.Interfaces.IService;

public interface IGoalService
{
    Task CompleteMilestoneAsync(Guid milestoneId, MilestoneStatus status);
    Task ExtendMilestoneAsync(Guid milestoneId, int extraDays);
    Task<Goal?> GetActiveGoalAsync();
    Task<Goal> CreateGoalAsync(string title, string? description, int milestoneDays,
        List<(string Title, int TargetCountPerWeek)> actions);

    Task UpdateGoalAsync(Guid goalId, string title, string? description);
    Task AddActionAsync(Guid milestoneId, string title, int targetCountPerWeek);
    Task UpdateActionAsync(Guid actionId, string title, int targetCountPerWeek);
    Task RemoveActionAsync(Guid actionId);
}