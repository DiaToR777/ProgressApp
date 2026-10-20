using ProgressApp.Domain.Models.Goals;
using ProgressApp.Domain.Models.Journal;

namespace ProgressApp.WpfUI.Localization.Helpers;

public static class EnumLocalizationRegistry
{
    private static readonly Dictionary<Enum, string> _keys = new()
    {
        [DayResult.Success] = "Result_Success",
        [DayResult.Relapse] = "Result_Relapse",
        [DayResult.PartialSuccess] = "Result_Partial",

        [MilestoneStatus.Active] = "Milestone_Active",
        [MilestoneStatus.Success] = "Milestone_Success",
        [MilestoneStatus.Fail] = "Milestone_Fail",
        [MilestoneStatus.Paused] = "Milestone_Paused",

        [GoalStatus.Active] = "Goal_Active",
        [GoalStatus.Completed] = "Goal_Completed",
        [GoalStatus.Archived] = "Goal_Archived",
    };

    public static string? TryGetKey(Enum value) =>
        _keys.TryGetValue(value, out var key) ? key : null;
}