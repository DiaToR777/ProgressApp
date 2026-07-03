using ProgressApp.Domain.Models.Goals;

namespace ProgressApp.Domain.Interfaces.IService;

public interface IGoalService
{
    Task<Goal?> GetActiveGoalAsync(Guid userId);
    Task<Goal> CreateGoalAsync(Guid userId, string title, string? description, int milestoneDays);
    Task UpdateGoalAsync(Goal goal);
}