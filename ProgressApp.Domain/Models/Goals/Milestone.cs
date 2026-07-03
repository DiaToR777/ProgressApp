using ProgressApp.Domain.Models.Journal;

namespace ProgressApp.Domain.Models.Goals;

public class Milestone
{
    public Guid Id { get; set; }
    public Guid GoalId { get; set; }
    
    public DateTime StartDate { get; set; }
    public int TargetDays { get; set; }
    
    public DateTime EndDate => StartDate.AddDays(TargetDays);
    
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Active;

    public Goal Goal { get; set; } = null!;
    public List<GoalAction> Actions { get; set; } = new();
    public List<DailyCheckin> Checkins { get; set; } = new();
}