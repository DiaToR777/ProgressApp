namespace ProgressApp.Domain.Models.Goals;

public class Goal
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public GoalStatus Status { get; set; } = GoalStatus.Active;
    public DateTime CreatedAt { get; set; }

    public List<Milestone> Milestones { get; set; } = new();
    public User.User User { get; set; } = null!;
    public Guid UserId { get; set; }
}