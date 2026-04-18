using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Heatmap;
using ProgressApp.WpfUI.Localization.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProgressApp.WpfUI.ViewModels.Analytics.Heatmap
{
    public class HeatmapViewModel : ViewModelBase
    {
        private readonly IAnalyticsService _analyticsService;
        public string[] DayLabels => CultureHelper.GetAbbreviatedDayNames();

        private DateTime _currentDate = DateTime.Today;
        public ObservableCollection<List<DayCell>> Weeks { get; } = new();

        public bool ShowWeekLabels => _selectedRange != HeatmapRange.AllTime;

        private DayCell? _selectedCell;
        public DayCell? SelectedCell
        {
            get => _selectedCell;
            set => SetProperty(ref _selectedCell, value);
        }

        public IEnumerable<LocalizedEnum<HeatmapRange>> Ranges { get; }

        private HeatmapRange _selectedRange = HeatmapRange.Week;
        public HeatmapRange SelectedRange
        {
            get => _selectedRange;
            set
            {
                if (SetProperty(ref _selectedRange, value))
                {
                    OnPropertyChanged(nameof(CellSize));
                    OnPropertyChanged(nameof(CellMargin));
                    OnPropertyChanged(nameof(ShowWeekLabels));

                    _ = LoadAsync(value);
                }
            }
        }


        public int CellSize => _selectedRange switch
        {
            HeatmapRange.Week => 40,
            HeatmapRange.Month => 30,
            _ => 30
        };

        public int CellMargin => _selectedRange switch
        {
            HeatmapRange.Week => 4,
            HeatmapRange.Month => 3,
            _ => 3
        };

        public ICommand CellClickCommand { get; }


        //TODO heatmap allTime stats, add loading state, add error handling and logging
        public HeatmapViewModel(IAnalyticsService analyticsService)
        {
            Ranges = Enum.GetValues(typeof(HeatmapRange))
                .Cast<HeatmapRange>()
                .Select(r => new LocalizedEnum<HeatmapRange>(r))
                .ToList();

            _analyticsService = analyticsService;
            CellClickCommand = new RelayCommand(cell => SelectedCell = cell as DayCell);
            _ = LoadAsync(_selectedRange);
        }

        public async Task LoadAsync(HeatmapRange range)
        {
            if (range == HeatmapRange.AllTime)
            {
                await LoadAllTimeAsync();
                return;
            }
            var (from, to) = range switch
            {
                HeatmapRange.Week => GetCurrentWeek(),
                HeatmapRange.Month => GetCurrentMonthAligned(),
                _ => GetCurrentWeek()
            };

            var allCells = await _analyticsService.GetHeatmapCells(from, to);
            var lookup = allCells.ToDictionary(c => c.Date.Date);

            Weeks.Clear();
            SelectedCell = null;

            var current = from.Date;

            while (current <= to.Date)
            {
                var week = new List<DayCell>();

                for (int i = 0; i < 7 && current <= to.Date; i++)
                {
                    week.Add(lookup.TryGetValue(current, out var cell)
                        ? cell
                        : new DayCell { Date = current, Result = null });

                    current = current.AddDays(1);
                }

                Weeks.Add(week);
            }
        }
        private async Task LoadAllTimeAsync()
        {
            var cells = await _analyticsService.GetAllHeatmapCellsAsync();

            Weeks.Clear();
            SelectedCell = null;

            foreach (var chunk in cells.Chunk(7))
                Weeks.Add(chunk.ToList());
        }
        private (DateTime from, DateTime to) GetCurrentWeek()
        {
            var monday = _currentDate.AddDays(-(((int)_currentDate.DayOfWeek + 6) % 7));
            return (monday, monday.AddDays(6));
        }

        private (DateTime from, DateTime to) GetCurrentMonthAligned()
        {
            var firstDay = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            // Начало: Понедельник недели, на которую выпало 1-е число
            var start = firstDay.AddDays(-(((int)firstDay.DayOfWeek + 6) % 7));
            // Конец: Воскресенье недели, на которую выпало последнее число
            var end = lastDay.AddDays((7 - (int)lastDay.AddDays(1).DayOfWeek + 6) % 7);
            // Упростим: просто берем +6 дней от понедельника последней недели
            var lastMonday = lastDay.AddDays(-(((int)lastDay.DayOfWeek + 6) % 7));
            var finalEnd = lastMonday.AddDays(6);

            return (start, finalEnd);
        }
    }
}