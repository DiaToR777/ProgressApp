using System.ComponentModel.DataAnnotations;

namespace ProgressApp.Core.Models.Journal
{
    public class JournalEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        [MaxLength(1500)]
        public string Description { get; set; } = string.Empty;
        public DayResult Result { get; set; }
        public DateTime CreatedAt { get; set; }

    }

}
