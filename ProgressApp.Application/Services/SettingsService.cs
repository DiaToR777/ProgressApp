using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Settings;
using Serilog;

namespace ProgressApp.Application.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task<string> GetGoalAsync()
        {
            try
            {
                return await _settingsRepository.GetGoalAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load setting for key: {Key}", SettingsKeys.Goal);
                throw new AppException("Msg_ErrorLoadingSetting", isCritical: true, SettingsKeys.Goal);
            }
        }

        public async Task SaveGoalAsync(string goal)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(goal)) throw new AppException("Msg_GoalEmpty");

                await _settingsRepository.SaveGoalAsync(goal);
                Log.Information("All settings saved successfully.");

            }
            catch (Exception ex) when (ex is not AppException)
            {
                Log.Error(ex, "Fatal error while saving settings!");
                throw new AppException("Msg_SaveSettingsError", isCritical: true);
            }
        }
    }
}
