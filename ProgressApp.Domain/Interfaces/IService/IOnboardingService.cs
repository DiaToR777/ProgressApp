using ProgressApp.Domain.Models.User;

namespace ProgressApp.Domain.Interfaces.IService;

public interface IOnboardingService
{
    Task<Guid> CompleteOnboardingAsync(string username, string goalTitle, int milestoneDays);
    
}
