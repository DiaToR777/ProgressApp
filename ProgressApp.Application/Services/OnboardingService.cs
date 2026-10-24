using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.User;
using Serilog;

namespace ProgressApp.Application.Services;

public class OnboardingService : IOnboardingService
{
    private readonly IUserRepository _userRepository;
    private readonly IGoalService _goalService; //TODO add cqs

    public OnboardingService(IUserRepository userRepository, IGoalService goalService)
    {
        _userRepository = userRepository;
        _goalService = goalService;
    }

    
    public async Task<Guid> CompleteOnboardingAsync(string username,
        string goalTitle,
        string? goalDescription,
        int milestoneDays,
        List<(string Title, int TargetCountPerWeek)> actions)
    {
        try
        {
            var user = await _userRepository.CreateUserAsync(new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                CreatedAt = DateTime.Now
            });
        
            await _goalService.CreateGoalAsync(goalTitle,  goalDescription, milestoneDays, actions);

            Log.Information("Onboarding completed successfully for user {UserId}", user.Id);
        
            return user.Id;
        }
        catch (Exception ex) when (ex is not AppException)
        {
            Log.Error(ex, "Failed to complete onboarding for username {Username}", username);
            throw new AppException("Msg_FinishSetupError", isCritical: true);
        }
       
    }   
}