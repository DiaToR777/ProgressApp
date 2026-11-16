using ProgressApp.Domain.Models.Goals;

namespace ProgressApp.Domain.Interfaces.IRepository;

public interface IGoalRepository
{
    Task CloseMilestoneAsync(Guid milestoneId, MilestoneStatus milestoneStatus, GoalStatus goalStatus);
    Task<Milestone?> GetMilestoneByIdAsync(Guid milestoneId);
    Task UpdateMilestoneAsync(Milestone milestone);
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