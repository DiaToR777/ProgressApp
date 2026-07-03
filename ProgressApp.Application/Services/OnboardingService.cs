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

    public async Task<Guid> CompleteOnboardingAsync(string username, string goalTitle, int milestoneDays)
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

        await _goalRepository.CreateGoalAsync(goal);

        return user.Id;
    }}