
using ProgressApp.Domain.Models.Journal;

namespace ProgressApp.Domain.Models.Heatmap
{
    public class DayCell
    {
        public DateTime Date { get; init; }
        public string? Description { get; init; }
        public DayResult? Result { get; init; }
    }
}
