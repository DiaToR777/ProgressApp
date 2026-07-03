using ProgressApp.Domain.Models.Goals;

namespace ProgressApp.Domain.Interfaces.IRepository;

public interface IGoalRepository
{
    Task<Goal?> GetActiveGoalAsync(Guid userId);
    Task<Goal> CreateGoalAsync(Goal goal);
    Task UpdateGoalAsync(Goal goal);
    Task<Milestone?> GetActiveMilestoneAsync(Guid goalId);
    Task<Milestone> CreateMilestoneAsync(Milestone milestone);
}