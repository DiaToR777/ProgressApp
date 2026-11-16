namespace ProgressApp.Domain.Models.Goals;

public class MilestoneProgress
{
    public Guid MilestoneId { get; set; }
    public int TargetDays { get; set; }
    public int DaysElapsed { get; set; }
    public double DaysCompletionPercent { get; set; }
    public double ConsistencyPercent { get; set; }
    public bool IsExpired { get; set; }
    public MilestoneStatus SuggestedStatus { get; set; }
}