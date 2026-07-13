using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.Domain.Models.Journal;
using Serilog;

namespace ProgressApp.Application.Services
{
    public class CheckinService : ICheckinService
    {
        private readonly ICheckinRepository _checkinRepository;
        private readonly IGoalRepository _goalRepository;
        private readonly IUserRepository _userRepository;

        public CheckinService(ICheckinRepository checkinRepository, IGoalRepository goalRepository,
            IUserRepository userRepository)
        {
            _userRepository = userRepository;
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

        public async Task<List<GoalAction>> GetActiveActionsAsync()
        {
            try
            {
                var user = await _userRepository.GetUserAsync();
                if (user == null) return new List<GoalAction>();

                var goal = await _goalRepository.GetActiveGoalAsync(user.Id);
                if (goal == null) return new List<GoalAction>();

                var milestone = await _goalRepository.GetActiveMilestoneAsync(goal.Id);
                return milestone?.Actions ?? new List<GoalAction>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load active actions.");
                throw new AppException("Msg_ErrorLoadingActions", isCritical: true);
            }
        }

        public async Task SaveTodayAsync(string description, DayResult result, Dictionary<Guid, bool> actionCompletions)
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
                    Log.Fatal("SaveTodayAsync: No user found in database. Onboarding may not have completed.");
                    throw new AppException("Msg_UserNotFound", isCritical: true);
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

                if (milestone != null)
                {
                    foreach (var action in milestone.Actions)
                    {
                        checkin.ActionLogs.Add(new ActionLog
                        {
                            GoalActionId = action.Id,
                            IsCompleted = actionCompletions.GetValueOrDefault(action.Id, false)
                        });
                    }
                }

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

        public async Task<MilestoneProgress?> GetActiveMilestoneProgressAsync()
        {
            try
            {
                var user = await _userRepository.GetUserAsync();
                if (user == null) return null;

                var goal = await _goalRepository.GetActiveGoalAsync(user.Id);
                if (goal == null) return null;

                var milestone = await _goalRepository.GetActiveMilestoneAsync(goal.Id);
                if (milestone == null) return null;

                var today = DateTime.Today;
                var daysElapsed = (today.Date - milestone.StartDate.Date).Days + 1;
                daysElapsed = Math.Clamp(daysElapsed, 0, milestone.TargetDays);

                var daysCompletionPercent = milestone.TargetDays > 0
                    ? Math.Round((double)daysElapsed / milestone.TargetDays * 100, 1)
                    : 0;

                double totalScore = 0;
                int scoredDays = 0;

                if (milestone.Actions.Count > 0)
                {
                    var orderedCheckins = milestone.Checkins.OrderBy(c => c.Date).ToList();

                    foreach (var checkin in orderedCheckins)
                    {
                        if (checkin.Date.Date > today.Date) continue;

                        var score = CalculateDayScore(checkin, milestone, orderedCheckins);
                        totalScore += score;
                        scoredDays++;
                    }
                }
                else
                {
                    foreach (var checkin in milestone.Checkins)
                    {
                        totalScore += checkin.Result switch
                        {
                            DayResult.Success => 1.0,
                            DayResult.PartialSuccess => 0.5,
                            DayResult.Relapse => 0.0,
                            _ => 0.0
                        };
                        scoredDays++;
                    }
                }

                var consistencyPercent = scoredDays > 0
                    ? Math.Round(totalScore / scoredDays * 100, 1)
                    : 0;

                return new MilestoneProgress
                {
                    TargetDays = milestone.TargetDays,
                    DaysElapsed = daysElapsed,
                    DaysCompletionPercent = Math.Clamp(daysCompletionPercent, 0, 100),
                    ConsistencyPercent = Math.Clamp(consistencyPercent, 0, 100)
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to calculate milestone progress.");
                throw new AppException("Msg_ErrorLoadingGoalProgress", isCritical: true); //TODO
            }
        }

        private double CalculateDayScore(DailyCheckin checkin, Milestone milestone, List<DailyCheckin> allCheckins)
        {
            var dailyActions = milestone.Actions.Where(a => a.TargetCountPerWeek >= 7).ToList();
            var weeklyActions = milestone.Actions.Where(a => a.TargetCountPerWeek < 7).ToList();

            var dueActionsCount = dailyActions.Count;
            var earnedScore = checkin.ActionLogs
                .Count(l => l.IsCompleted && dailyActions.Any(a => a.Id == l.GoalActionId));

            var isWeekEnd = checkin.Date.DayOfWeek == DayOfWeek.Sunday
                            || checkin.Date.Date == milestone.StartDate.AddDays(milestone.TargetDays - 1).Date;

            if (isWeekEnd && weeklyActions.Count > 0)
            {
                var weekStart = checkin.Date.AddDays(-6);

                foreach (var action in weeklyActions)
                {
                    dueActionsCount++;

                    var completedThisWeek = allCheckins
                        .Where(c => c.Date.Date >= weekStart.Date && c.Date.Date <= checkin.Date.Date)
                        .SelectMany(c => c.ActionLogs)
                        .Count(l => l.GoalActionId == action.Id && l.IsCompleted);

                    if (completedThisWeek >= action.TargetCountPerWeek)
                        earnedScore += 1;
                }
            }

            return dueActionsCount > 0 ? earnedScore / (double)dueActionsCount : 0;
        }
    }
}