namespace ProgressApp.Domain.Interfaces.IRepository;

public interface ISettingsRepository
{
    Task<string> GetGoalAsync();
    Task SaveGoalAsync(string goal);

}
