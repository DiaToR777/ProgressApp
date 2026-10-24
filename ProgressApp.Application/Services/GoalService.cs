using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.Domain.Models.User;
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

    public async Task CompleteMilestoneAsync(Guid milestoneId, MilestoneStatus status)
    {
        if (status is not (MilestoneStatus.Success or MilestoneStatus.Fail))
            throw new AppException("Msg_InvalidMilestoneStatus", args: status);

        try
        {
            var goalStatus = status == MilestoneStatus.Success
                ? GoalStatus.Completed
                : GoalStatus.Archived;

            await _goalRepository.CloseMilestoneAsync(milestoneId, status, goalStatus);
            Log.Information("Milestone {Id} closed as {Status}, goal set to {GoalStatus}.",
                milestoneId, status, goalStatus);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            Log.Error(ex, "Failed to complete milestone {Id}", milestoneId);
            throw new AppException("Msg_ErrorCompletingMilestone", isCritical: true);
        }
    }

    public async Task ExtendMilestoneAsync(Guid milestoneId, int extraDays)
    {
        if (extraDays is < 1 or > 365)
            throw new AppException("Msg_InvalidExtraDays");

        try
        {
            var milestone = await _goalRepository.GetMilestoneByIdAsync(milestoneId);
            if (milestone == null) throw new AppException("Msg_MilestoneNotFound");

            milestone.TargetDays += extraDays;
            await _goalRepository.UpdateMilestoneAsync(milestone);
            Log.Information("Milestone {Id} extended by {Days} days.", milestoneId, extraDays);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            Log.Error(ex, "Failed to extend milestone {Id}", milestoneId);
            throw new AppException("Msg_ErrorExtendingMilestone", isCritical: true);
        }
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

    public async Task<Goal> CreateGoalAsync(
        string goalTitle,
        string? goalDescription,
        int milestoneDays,
        List<(string Title, int TargetCountPerWeek)> actions)
    {
        if (string.IsNullOrWhiteSpace(goalTitle))
            throw new AppException("Msg_GoalEmpty");

        User? user;
        try
        {
            user = await _userRepository.GetUserAsync();
            if (user == null)
                throw new AppException("Msg_UserNotFound");
        }
        catch (Exception ex) when (ex is not AppException)
        {
            Log.Error(ex, "Failed to get user while creating goal");
            throw new AppException("Msg_ErrorLoadingUser", isCritical: true);
        }

        try
        {
            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                UserId = user.Id, 
                Title = goalTitle,
                Description = goalDescription,
                Status = GoalStatus.Active,
                CreatedAt = DateTime.Now
            };

            var milestone = new Milestone
            {
                Id = Guid.NewGuid(),
                GoalId = goal.Id,
                StartDate = DateTime.Today,
                TargetDays = milestoneDays,
                Status = MilestoneStatus.Active
            };

            foreach (var (title, targetCountPerWeek) in actions)
            {
                milestone.Actions.Add(new GoalAction
                {
                    Id = Guid.NewGuid(),
                    MilestoneId = milestone.Id,
                    Title = title,
                    TargetCountPerWeek = targetCountPerWeek
                });
            }

            goal.Milestones.Add(milestone);

            var createdGoal = await _goalRepository.CreateGoalAsync(goal);
            Log.Information("Goal {GoalId} created successfully for user {UserId}", createdGoal.Id, user.Id);

            return createdGoal;
        }
        catch (Exception ex) when (ex is not AppException)
        {
            Log.Error(ex, "Failed to create goal for user {UserId}", user.Id);
            throw new AppException("Msg_CreateGoalError", isCritical: true);
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