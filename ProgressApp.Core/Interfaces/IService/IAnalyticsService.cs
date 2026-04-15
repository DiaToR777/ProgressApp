
namespace ProgressApp.Core.Interfaces.IService
{
    public interface IAnalyticsService
    {
        Task<int> GetCurrentStreakAsync();
    }
}
