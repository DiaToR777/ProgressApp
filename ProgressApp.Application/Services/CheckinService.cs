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
                var totalDaysInMilestone = milestone.TargetDays;
                var daysElapsed = (today.Date - milestone.StartDate.Date).Days + 1;
                daysElapsed = Math.Clamp(daysElapsed, 0, totalDaysInMilestone);

                var daysCompletionPercent = totalDaysInMilestone > 0
                    ? Math.Round((double)daysElapsed / totalDaysInMilestone * 100, 1)
                    : 0;

                double totalScore = 0;
                int scoredDays = 0;

                if (milestone.Actions.Count > 0)
                {
                    var lastDayToCalculate = today.Date < milestone.StartDate.AddDays(totalDaysInMilestone).Date
                        ? today.Date
                        : milestone.StartDate.AddDays(totalDaysInMilestone - 1).Date;

                    var checkinDict = milestone.Checkins.ToDictionary(c => c.Date.Date);

                    for (var date = milestone.StartDate.Date; date <= lastDayToCalculate; date = date.AddDays(1))
                    {
                        checkinDict.TryGetValue(date, out var checkin);
    
                        var score = CalculateDayScoreDynamic(date, checkin, milestone, checkinDict);
                        totalScore += score;
                        scoredDays++;
                    }
                }
                else
                {
                    var checkinDict = milestone.Checkins.ToDictionary(c => c.Date.Date);
                    var lastDayToCalculate = today.Date < milestone.StartDate.AddDays(totalDaysInMilestone).Date
                        ? today.Date
                        : milestone.StartDate.AddDays(totalDaysInMilestone - 1).Date;

                    for (var date = milestone.StartDate.Date; date <= lastDayToCalculate; date = date.AddDays(1))
                    {
                        if (checkinDict.TryGetValue(date, out var checkin))
                        {
                            totalScore += checkin.Result switch
                            {
                                DayResult.Success => 1.0,
                                DayResult.PartialSuccess => 0.5,
                                DayResult.Relapse => 0.0,
                                _ => 0.0
                            };
                        }
                        else
                        {
                            totalScore += 0.0;
                        }

                        scoredDays++;
                    }
                }

                var consistencyPercent = scoredDays > 0
                    ? Math.Round(totalScore / scoredDays * 100, 1)
                    : 0;

                return new MilestoneProgress
                {
                    TargetDays = totalDaysInMilestone,
                    DaysElapsed = daysElapsed,
                    DaysCompletionPercent = Math.Clamp(daysCompletionPercent, 0, 100),
                    ConsistencyPercent = Math.Clamp(consistencyPercent, 0, 100)
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to calculate milestone progress.");
                throw new AppException("Msg_ErrorLoadingGoalProgress", isCritical: true);
            }
        }

        private double CalculateDayScoreDynamic(
            DateTime currentDate,
            DailyCheckin? currentCheckin,
            Milestone milestone,
            Dictionary<DateTime, DailyCheckin> checkinDict) 
        {
            var actions = milestone.Actions;
            if (actions.Count == 0) return 0;

            double earnedScore = 0;
            double expectedScore = 0;

            for (int i = 0; i < actions.Count; i++)
            {
                var action = actions[i];

                if (action.TargetCountPerWeek >= 7)
                {
                    expectedScore += 1.0;
                    if (currentCheckin != null)
                    {
                        var logs = currentCheckin.ActionLogs;
                        for (int j = 0; j < logs.Count; j++)
                        {
                            if (logs[j].GoalActionId == action.Id && logs[j].IsCompleted)
                            {
                                earnedScore += 1.0;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // 2. Еженедельная задача (плавный расчет)
                    expectedScore += 1.0;

                    // Вычисляем границы текущей недели без сложных формул
                    int daysSinceStart = (currentDate.Date - milestone.StartDate.Date).Days;
                    int daysPassedInThisWeek = (daysSinceStart % 7) + 1; // День текущей недели (1 до 7)

                    DateTime weekStart = currentDate.Date.AddDays(-(daysPassedInThisWeek - 1));

                    // Считаем выполнения за текущую неделю БЕЗ LINQ (пробегаемся по словарю за 7 дней)
                    int completedThisWeekSoFar = 0;
                    for (int dayOffset = 0; dayOffset < daysPassedInThisWeek; dayOffset++)
                    {
                        DateTime dayToCheck = weekStart.AddDays(dayOffset);
                        if (checkinDict.TryGetValue(dayToCheck, out var historicalCheckin))
                        {
                            var logs = historicalCheckin.ActionLogs;
                            for (int j = 0; j < logs.Count; j++)
                            {
                                if (logs[j].GoalActionId == action.Id && logs[j].IsCompleted)
                                {
                                    completedThisWeekSoFar++;
                                    break;
                                }
                            }
                        }
                    }

                    double proportionalTarget = (double)action.TargetCountPerWeek * daysPassedInThisWeek / 7.0;

                    if (completedThisWeekSoFar >= action.TargetCountPerWeek)
                    {
                        earnedScore += 1.0;
                    }
                    else
                    {
                        double progressRatio = completedThisWeekSoFar / proportionalTarget;
                        earnedScore += Math.Min(progressRatio, 1.0);
                    }
                }
            }

            return expectedScore > 0 ? Math.Clamp(earnedScore / expectedScore, 0.0, 1.0) : 0;
        }
    }
}