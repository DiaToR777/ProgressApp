namespace ProgressApp.Domain.Models.Goals;

public class GoalAction
{
    public Guid Id { get; set; }
    public Guid MilestoneId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public int TargetCountPerWeek { get; set; } 

    public Milestone Milestone { get; set; } = null!;
    public List<ActionLog> Logs { get; set; } = new();
}
