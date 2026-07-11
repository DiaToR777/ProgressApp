namespace ProgressApp.Domain.Models.Goals;

public class MilestoneProgress
{
    public int TargetDays { get; set; }
    public int DaysElapsed { get; set; }
    public double DaysCompletionPercent { get; set; }
    public double ConsistencyPercent { get; set; }    
}