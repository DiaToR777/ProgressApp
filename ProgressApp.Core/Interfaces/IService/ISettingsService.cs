
namespace ProgressApp.Core.Interfaces.IService
{
    public interface ISettingsService
    {
        Task<string> GetGoalAsync();
        Task SaveGoalAsync(string goal);
    }
}
