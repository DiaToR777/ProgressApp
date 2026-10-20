using ProgressApp.Domain.Models.Goals;

namespace ProgressApp.Domain.Interfaces.IRepository;

public interface IGoalRepository
{
    Task<Milestone?> GetActiveMilestoneWithDetailsAsync(Guid goalId);
    Task<Goal?> GetGoalByIdAsync(Guid goalId); 
    Task<Goal?> GetActiveGoalAsync(Guid userId);
    Task<Goal> CreateGoalAsync(Goal goal);
    Task UpdateGoalAsync(Goal goal);
    Task<Milestone?> GetActiveMilestoneAsync(Guid goalId);
    Task<Milestone> CreateMilestoneAsync(Milestone milestone);
    Task<GoalAction> AddActionAsync(GoalAction action);
    Task<GoalAction?> GetActionByIdAsync(Guid actionId);
    Task UpdateActionAsync(GoalAction action);
    Task RemoveActionAsync(Guid actionId);
}