namespace ProgressApp.Core.Models.Journal
{
    public class JournalEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public DayResult Result { get; set; }
        public DateTime CreatedAt { get; set; }

    }

}
