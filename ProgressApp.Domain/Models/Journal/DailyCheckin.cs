using ProgressApp.Domain.Models.Goals;

namespace ProgressApp.Domain.Models.Journal;

public class DailyCheckin
{
    public Guid Id { get; set; }
    public Guid? MilestoneId { get; set; }
    
    public DayResult? Result { get; set; }  
    public DateTime Date { get; set; }
    
    public string? Description { get; set; } 

    public Milestone? Milestone { get; set; }
    
    public List<ActionLog> ActionLogs { get; set; } = new();
}