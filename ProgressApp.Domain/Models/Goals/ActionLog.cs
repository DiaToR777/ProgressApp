using ProgressApp.Domain.Models.Journal;

namespace ProgressApp.Domain.Models.Goals;

public class ActionLog
{
    public Guid Id { get; set; }
    public Guid DailyCheckinId { get; set; }
    public Guid GoalActionId { get; set; }
    
    public bool IsCompleted { get; set; }

    public DailyCheckin? DailyCheckin { get; set; }
    public GoalAction GoalAction { get; set; } = null!;
}