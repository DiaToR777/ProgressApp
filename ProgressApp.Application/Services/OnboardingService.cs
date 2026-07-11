using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.Domain.Models.User;

namespace ProgressApp.Application.Services;

public class OnboardingService : IOnboardingService
{
    private readonly IUserRepository _userRepository;
    private readonly IGoalRepository _goalRepository;

    public OnboardingService(IUserRepository userRepository, IGoalRepository goalRepository)
    {
        _userRepository = userRepository;
        _goalRepository = goalRepository;
    }

    public async Task<Guid> CompleteOnboardingAsync(string username,
        string goalTitle,
        string? goalDescription,
        int milestoneDays,
        List<(string Title, int TargetCountPerWeek)> actions)
    {
        var user = await _userRepository.CreateUserAsync(new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            CreatedAt = DateTime.Now
        });

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
        await _goalRepository.CreateGoalAsync(goal);

        return user.Id;
    }
}