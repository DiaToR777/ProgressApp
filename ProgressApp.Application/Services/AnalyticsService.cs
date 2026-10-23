using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.Domain.Models.Heatmap;
using ProgressApp.Domain.Models.Journal;
using Serilog;

namespace ProgressApp.Application.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _analyticsRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGoalRepository _goalRepository;

        private const double SuccessThresholdPercent = 70;
        
        public AnalyticsService(IAnalyticsRepository analyticsRepository, 
            IUserRepository userRepository, IGoalRepository goalRepository)
        {
            _userRepository = userRepository;
            _goalRepository = goalRepository;
            _analyticsRepository = analyticsRepository;
        }

        public async Task<DateTime?> GetFirstEntryDateAsync()
        {
            try
            {
                var firstEntry = await _analyticsRepository.GetFirstEntryDateAsync();

                Log.Debug("AnalyticsService: First entry date: {Date}", firstEntry?.Date);
                return firstEntry?.Date;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AnalyticsService: Failed to get first entry date.");
                throw new AppException("Msg_ErrorLoadingHeatmapData", isCritical: true);
                //TODO Custom ServiceResult 
            }
        }

        public async Task<List<DayCell>> GetHeatmapCellsAsync(DateTime from, DateTime to)
        {
            try
            {
                var entries = await _analyticsRepository.GetEntriesByDateRangeAsync(from, to);
                var lookup = entries.ToDictionary(e => e.Date.Date);

                var cells = new List<DayCell>();
                for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
                {
                    var entry = lookup.GetValueOrDefault(date);

                    double? score = null;
                    if (entry != null)
                    {
                        score = entry.ActionLogs.Count > 0
                            ? (double)entry.ActionLogs.Count(l => l.IsCompleted) / entry.ActionLogs.Count
                            : entry.Result switch
                            {
                                DayResult.Success => 1.0,
                                DayResult.PartialSuccess => 0.5,
                                DayResult.Relapse => 0.0,
                                _ => (double?)null
                            };
                    }

                    cells.Add(new DayCell
                    {
                        Date = date,
                        Result = entry?.Result,
                        Description = entry?.Description,
                        Score = score,
                        ActionLogs = entry?.ActionLogs
                            .Select(l => new ActionLogInfo { Title = l.GoalAction.Title, IsCompleted = l.IsCompleted })
                            .ToList() ?? new List<ActionLogInfo>()
                    });
                }

                Log.Debug("AnalyticsService: Fetched {Count} heatmap cells from {From} to {To}.",
                    cells.Count, from.ToShortDateString(), to.ToShortDateString());
                return cells;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AnalyticsService: Failed to get heatmap cells from {From} to {To}.", from, to);
                throw new AppException("Msg_ErrorLoadingHeatmapData", isCritical: true);
            }
        }

        public async Task<int> GetCurrentStreakAsync()
        {
            try
            {
                var entries = await _analyticsRepository.GetEntriesForStreakAsync();

                var streak = CalculateCurrentStreak(entries.Select(e => (e.Date, e.Result)));
                Log.Debug("AnalyticsService: Current streak: {Streak} days.", streak);
                return streak;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AnalyticsService: Failed to calculate current streak.");
                throw new AppException("Msg_ErrorLoadingStreak", isCritical: true);
            }
        }

        private int CalculateCurrentStreak(IEnumerable<(DateTime Date, DayResult Result)> entries)
        {
            var entriesList = entries.ToList();
            if (!entriesList.Any()) return 0;

            var today = DateTime.Today;

            if (entriesList[0].Date.Date < today.AddDays(-1))
                return 0;

            int streak = 0;
            DateTime expectedDate = entriesList[0].Date.Date;

            foreach (var entry in entriesList)
            {
                if (entry.Result == DayResult.Relapse)
                    break;

                if (entry.Date.Date == expectedDate)
                {
                    streak++;
                    expectedDate = expectedDate.AddDays(-1);
                }
                else if (entry.Date.Date < expectedDate)
                    break;
            }

            return streak;
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
                var allCheckins = milestone.Checkins.ToList();
                var checkinsByDate = allCheckins.ToDictionary(c => c.Date.Date);

                for (var date = milestone.StartDate.Date;
                     date <= today.Date && date < milestone.StartDate.Date.AddDays(milestone.TargetDays);
                     date = date.AddDays(1))
                {
                    if (checkinsByDate.TryGetValue(date, out var checkin))
                        totalScore += CalculateDayScore(checkin, milestone, allCheckins);
                }

                var consistencyPercent = daysElapsed > 0
                    ? Math.Round(totalScore / daysElapsed * 100, 1)
                    : 0;

                var isExpired = today.Date >= milestone.StartDate.Date.AddDays(milestone.TargetDays);
                var clampedConsistency = Math.Clamp(consistencyPercent, 0, 100);

                return new MilestoneProgress
                {
                    MilestoneId = milestone.Id,
                    TargetDays = milestone.TargetDays,
                    DaysElapsed = daysElapsed,
                    DaysCompletionPercent = Math.Clamp(daysCompletionPercent, 0, 100),
                    ConsistencyPercent = clampedConsistency,
                    IsExpired = isExpired,
                    SuggestedStatus = clampedConsistency >= SuccessThresholdPercent
                        ? MilestoneStatus.Success
                        : MilestoneStatus.Fail
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to calculate milestone progress.");
                throw new AppException("Msg_ErrorLoadingGoalProgress", isCritical: true);
            }
        }

        private double CalculateDayScore(DailyCheckin checkin, Milestone milestone, List<DailyCheckin> allCheckins)
        {
            if (checkin.ActionLogs.Count == 0)
            {
                return checkin.Result switch
                {
                    DayResult.Success => 1.0,
                    DayResult.PartialSuccess => 0.5,
                    DayResult.Relapse => 0.0,
                    _ => 0.0
                };
            }

            var loggedActionIds = checkin.ActionLogs.Select(l => l.GoalActionId).ToHashSet();
            var dayActions = milestone.Actions.Where(a => loggedActionIds.Contains(a.Id)).ToList();

            if (dayActions.Count == 0) return 1.0;

            var dailyActions = dayActions.Where(a => a.TargetCountPerWeek >= 7).ToList();
            var weeklyActions = dayActions.Where(a => a.TargetCountPerWeek < 7).ToList();

            var dueActionsCount = dailyActions.Count;
            var earnedScore = (double)checkin.ActionLogs
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

            return dueActionsCount > 0 ? earnedScore / dueActionsCount : 1.0;
        }
    }
}