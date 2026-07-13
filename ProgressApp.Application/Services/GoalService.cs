using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Goals;
using Serilog;

namespace ProgressApp.Application.Services;

public class GoalService : IGoalService
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUserRepository _userRepository;

    public GoalService(IGoalRepository goalRepository, IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _goalRepository = goalRepository;
    }

    public async Task<Goal?> GetActiveGoalAsync()
    {
        try
        {
            var user = await _userRepository.GetUserAsync();
            if (user == null) return null;
            return await _goalRepository.GetActiveGoalAsync(user.Id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load active goal for user");
            throw new AppException("Msg_ErrorLoadingGoal", isCritical: true);
        }
    }

    public async Task<Goal> CreateGoalAsync(Guid userId, string title, string? description, int milestoneDays)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new AppException("Msg_GoalEmpty");

        try
        {
            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Description = description,
                Status = GoalStatus.Active,
                CreatedAt = DateTime.Now
            };

            goal.Milestones.Add(new Milestone
            {
                Id = Guid.NewGuid(),
                GoalId = goal.Id,
                StartDate = DateTime.Today,
                TargetDays = milestoneDays,
                Status = MilestoneStatus.Active
            });

            return await _goalRepository.CreateGoalAsync(goal);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            Log.Error(ex, "Failed to create goal for user {UserId}", userId);
            throw new AppException("Msg_SaveGoalError", isCritical: true);
        }
    }

    public Task UpdateGoalAsync(Goal goal)
    {
        throw new NotImplementedException();
    }
}