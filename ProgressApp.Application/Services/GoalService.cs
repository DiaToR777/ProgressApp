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
    
    public async Task AddActionAsync(Guid milestoneId, string title, int targetCountPerWeek)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new AppException("Msg_ActionTitleEmpty");

        try
        {
            var action = new GoalAction
            {
                Id = Guid.NewGuid(),
                MilestoneId = milestoneId,
                Title = title,
                TargetCountPerWeek = Math.Clamp(targetCountPerWeek, 1, 7)
            };

            await _goalRepository.AddActionAsync(action);
            Log.Information("Action '{Title}' added to milestone {MilestoneId}.", title, milestoneId);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            Log.Error(ex, "Failed to add action to milestone {MilestoneId}", milestoneId);
            throw new AppException("Msg_ErrorAddingAction", isCritical: true);
        }
    }
    
    public async Task UpdateActionAsync(Guid actionId, string title, int targetCountPerWeek)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new AppException("Msg_ActionTitleEmpty");

        try
        {
            var action = await _goalRepository.GetActionByIdAsync(actionId);
            if (action == null)
                throw new AppException("Msg_ActionNotFound");

            action.Title = title;
            action.TargetCountPerWeek = Math.Clamp(targetCountPerWeek, 1, 7);

            await _goalRepository.UpdateActionAsync(action);
            Log.Information("Action {ActionId} updated.", actionId);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            Log.Error(ex, "Failed to update action {ActionId}", actionId);
            throw new AppException("Msg_ErrorUpdatingAction", isCritical: true);
        }
    }

    public async Task RemoveActionAsync(Guid actionId)
    {
        try
        {
            await _goalRepository.RemoveActionAsync(actionId);
            Log.Information("Action {ActionId} and its history removed.", actionId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to remove action {ActionId}", actionId);
            throw new AppException("Msg_ErrorRemovingAction", isCritical: true);
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

    public async Task UpdateGoalAsync(Guid goalId, string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new AppException("Msg_GoalEmpty");

        try
        {
            var goal = await _goalRepository.GetGoalByIdAsync(goalId);
            if (goal == null)
                throw new AppException("Msg_GoalNotFound");

            goal.Title = title;
            goal.Description = description;

            await _goalRepository.UpdateGoalAsync(goal);
            Log.Information("Goal {GoalId} updated successfully.", goalId);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            Log.Error(ex, "Failed to update goal {GoalId}", goalId);
            throw new AppException("Msg_SaveGoalError", isCritical: true);
        }
    }
}