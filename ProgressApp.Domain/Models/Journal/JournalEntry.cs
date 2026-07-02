namespace ProgressApp.Domain.Models.Journal;

public class JournalEntry
{
    public int Id { get; set; }
    public DateTime Date { get; set; } //TODO DATEONLY

    public string Description { get; set; } = string.Empty;
    public DayResult Result { get; set; }
    public DateTime CreatedAt { get; set; }

}
