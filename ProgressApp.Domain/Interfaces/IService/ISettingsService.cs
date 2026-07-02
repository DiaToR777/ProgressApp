namespace ProgressApp.Domain.Interfaces.IService;

public interface ISettingsService
{
    Task<string> GetGoalAsync();
    Task SaveGoalAsync(string goal);
}
