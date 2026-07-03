using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Journal;
using Serilog;

namespace ProgressApp.Application.Services
{
    public class CheckinService : ICheckinService
    {
        private readonly ICheckinRepository _checkinRepository;
        private readonly IGoalRepository _goalRepository;
        private readonly IUserRepository _userRepository;

        public CheckinService(ICheckinRepository checkinRepository, IGoalRepository goalRepository, IUserRepository userRepository)
        {
            _userRepository =  userRepository;
            _goalRepository = goalRepository;
            _checkinRepository = checkinRepository;
        }

        public async Task<DailyCheckin?> GetTodayAsync()
        {
            try
            {
                return await _checkinRepository.GetTodayAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while checking for today's entry in database.");
                throw new AppException("Msg_ErrorLoadingData", isCritical: true);
            }
        }

        public async Task SaveTodayAsync(string description, DayResult result)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                Log.Warning("Attempted to save today's entry with empty description.");
                throw new AppException("Msg_DescriptionEmpty");
            }

            try
            {
                var user = await _userRepository.GetUserAsync();
                if (user == null)
                {
                    //todo nullHandling
                }
                
                var goal = await _goalRepository.GetActiveGoalAsync(user.Id);
                var milestone = goal != null
                    ? await _goalRepository.GetActiveMilestoneAsync(goal.Id)
                    : null;
                
                var checkin = new DailyCheckin
                {
                    Date = DateTime.Today,
                    Description = description,
                    Result = result,
                    MilestoneId = milestone?.Id
                };

                await _checkinRepository.SaveTodayAsync(checkin);

                Log.Information("Entry saved successfully.");
            }
            catch (Exception ex) when (ex is not AppException)
            {
                Log.Error(ex, "Error occurred while saving today's entry");
                throw new AppException("Msg_SaveEntryError", isCritical: true);
            }
        }

        public async Task<List<DailyCheckin>> GetAllEntriesAsync()
        {
            try
            {
                var entries = await _checkinRepository.GetAllEntriesAsync();
                Log.Debug("Fetched {Count} entries from database.", entries.Count);
                return entries;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch all journal entries.");
                throw new AppException("Msg_ErrorLoadingData", isCritical: true);
            }
        }
    }
}