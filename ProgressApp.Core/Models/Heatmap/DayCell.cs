using ProgressApp.Core.Models.Journal;

namespace ProgressApp.Core.Models.Heatmap
{
    public class DayCell
    {
        public DateTime Date { get; init; }
        public string? Description { get; init; }
        public DayResult? Result { get; init; }
    }
}
