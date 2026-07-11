using ProgressApp.Domain.Models.User;

namespace ProgressApp.Domain.Interfaces.IService;

public interface IOnboardingService
{
    Task<Guid> CompleteOnboardingAsync(
        string username,
        string goalTitle,
        string? goalDescription,
        int milestoneDays,
        List<(string Title, int TargetCountPerWeek)> actions);
    
}
