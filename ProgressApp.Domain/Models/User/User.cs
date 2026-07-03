using ProgressApp.Domain.Models.Goals;

namespace ProgressApp.Domain.Models.User;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public List<Goal> Goals { get; set; } = new();
}