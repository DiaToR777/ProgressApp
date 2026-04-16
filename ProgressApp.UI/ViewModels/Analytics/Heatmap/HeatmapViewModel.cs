using ProgressApp.Core.Models.Heatmap;
using System.Collections.ObjectModel;
using ProgressApp.Core.Interfaces.IService;

namespace ProgressApp.WpfUI.ViewModels.Analytics.Heatmap
{
    public class HeatmapViewModel : ViewModelBase
    {
        public ObservableCollection<List<DayCell>> Weeks { get; } = new();
        private readonly IAnalyticsService _analyticsService;
        //TODO add logging
        //and error handling
        //and heatmap for 7d/30d/all time

        public HeatmapViewModel(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService; 
        }

        public async Task LoadAsync(DateTime from, DateTime to)
        {
           var allCells = await _analyticsService.GetHeatmapCells(from, to);

            Weeks.Clear();

            var start = from.Date;
            while (start.DayOfWeek != DayOfWeek.Monday)
                start = start.AddDays(-1);

            var current = start;

            var lookup = allCells.ToDictionary(c => c.Date.Date);

            while (current <= to.Date || current.DayOfWeek != DayOfWeek.Monday)
            {
                var week = new List<DayCell>();

                for (int i = 0; i < 7; i++)
                {
                    if (!lookup.TryGetValue(current, out var cell))
                    {
                        cell = new DayCell { Date = current, Result = null };
                    }

                    week.Add(cell);
                    current = current.AddDays(1);
                }

                Weeks.Add(week);

                if (current > to.Date && current.DayOfWeek == DayOfWeek.Monday)
                    break;
            }
        }
    }
}
