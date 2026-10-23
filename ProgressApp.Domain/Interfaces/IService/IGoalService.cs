using ProgressApp.Domain.Models.Goals;

namespace ProgressApp.Domain.Interfaces.IService;

public interface IGoalService
{
    Task CompleteMilestoneAsync(Guid milestoneId, MilestoneStatus status);
    Task ExtendMilestoneAsync(Guid milestoneId, int extraDays);
    Task<Goal?> GetActiveGoalAsync();
    Task<Goal> CreateGoalAsync(Guid userId, string title, string? description, int milestoneDays);
    Task UpdateGoalAsync(Guid goalId, string title, string? description);
    Task AddActionAsync(Guid milestoneId, string title, int targetCountPerWeek);
    Task UpdateActionAsync(Guid actionId, string title, int targetCountPerWeek);
    Task RemoveActionAsync(Guid actionId);
}