using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Heatmap;
using ProgressApp.WpfUI.Localization.Helpers;
using ProgressApp.WpfUI.Localization.Managers;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProgressApp.WpfUI.ViewModels.Analytics.Heatmap
{
    public class HeatmapViewModel : ViewModelBase
    {
        private DateOnly? _firstEntryDate;

        private readonly IAnalyticsService _analyticsService;
        private readonly IMessageService _messageService;
        public string[] DayLabels => CultureHelper.GetAbbreviatedDayNames();

        private DateTime _currentDate = DateTime.Today;
        public ObservableCollection<List<DayCell>> Weeks { get; } = new();

        private DayCell? _selectedCell;
        public DayCell? SelectedCell
        {
            get => _selectedCell;
            set => SetProperty(ref _selectedCell, value);
        }

        public string PeriodTitle => _selectedRange switch
        {
            HeatmapRange.Week => $"{GetCurrentWeek().from.ToString("dd MMM", TranslationSource.Instance.CurrentCulture)} — {GetCurrentWeek().to.ToString("dd MMM yyyy", TranslationSource.Instance.CurrentCulture)}",
            HeatmapRange.Month => _currentDate.ToString("MMMM yyyy", TranslationSource.Instance.CurrentCulture),
            HeatmapRange.AllTime => string.Empty,
            _ => string.Empty
        };
        public bool ShowNavigation => _selectedRange switch
        {
            HeatmapRange.AllTime => _firstEntryDate.HasValue && _firstEntryDate.Value.Year < DateTime.Today.Year,
            _ => true
        };

        public ICommand PreviousPeriodCommand { get; }
        public ICommand NextPeriodCommand { get; }

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
                    OnPropertyChanged(nameof(ShowNavigation));

                    _ = LoadAsync(value);
                }
            }
        }

        public int CellSize => _selectedRange switch
        {
            HeatmapRange.Week => 40,
            HeatmapRange.Month => 30,
            HeatmapRange.AllTime => 13,
            _ => 30
        };

        public int CellMargin => _selectedRange switch
        {
            HeatmapRange.Week => 4,
            HeatmapRange.Month => 3,
            HeatmapRange.AllTime => 1,
            _ => 3
        };

        private void NotifyPeriodChanged()
        {
            OnPropertyChanged(nameof(PeriodTitle));
            OnPropertyChanged(nameof(ShowNavigation));
        }

        public ICommand CellClickCommand { get; }

        public HeatmapViewModel(IAnalyticsService analyticsService, IMessageService messageService)
        {
            _messageService = messageService;   
            _analyticsService = analyticsService;

            Ranges = Enum.GetValues(typeof(HeatmapRange))
                .Cast<HeatmapRange>()
                .Select(r => new LocalizedEnum<HeatmapRange>(r))
                .ToList();

            GetFirstEntryDate();

            CellClickCommand = new RelayCommand(cell => SelectedCell = cell as DayCell);
            _ = LoadAsync(_selectedRange);

            PreviousPeriodCommand = new RelayCommand(_ =>
            {
                _currentDate = _selectedRange switch
                {
                    HeatmapRange.Week => _currentDate.AddDays(-7),
                    HeatmapRange.Month => _currentDate.AddMonths(-1),
                    HeatmapRange.AllTime => _currentDate.AddYears(-1),
                    _ => _currentDate
                };
                NotifyPeriodChanged();
                _ = LoadAsync(_selectedRange);

            }, _ => CanGoBack());


            NextPeriodCommand = new RelayCommand(_ =>
                {
                    _currentDate = _selectedRange switch
                    {
                        HeatmapRange.Week => _currentDate.AddDays(7),
                        HeatmapRange.Month => _currentDate.AddMonths(1),
                        HeatmapRange.AllTime => _currentDate.AddYears(1),
                        _ => _currentDate
                    };
                    NotifyPeriodChanged();
                    _ = LoadAsync(_selectedRange);
                }, _ => CanGoForward());
        }

        private async void GetFirstEntryDate() 
        {
            try
            {
                Log.Debug("HeatmapViewModel: Fetching first entry date.");
                var firstEntryDate = await _analyticsService.GetFirstEntryDateAsync();
                _firstEntryDate = firstEntryDate.HasValue ? DateOnly.FromDateTime(firstEntryDate.Value) : (DateOnly?)null;
                Log.Debug("HeatmapViewModel: First entry date: {Date}", _firstEntryDate);
            }
            catch (AppException ex)
            {
                Log.Error(ex, "HeatmapViewModel: Failed to get first entry date.");
                await _messageService.ShowErrorAsync(ex);
            }
        }

        private bool CanGoBack()
        {
            if (!_firstEntryDate.HasValue) return false;

            DateOnly firstDate = _firstEntryDate.Value;

            if (_selectedRange == HeatmapRange.AllTime)
                return _currentDate.Year > firstDate.Year;

            DateOnly currentViewStart = _selectedRange == HeatmapRange.Week
                ? DateOnly.FromDateTime(GetCurrentWeek().from)
                : new DateOnly(_currentDate.Year, _currentDate.Month, 1);

            return currentViewStart > firstDate;
        }

        private bool CanGoForward()
        {
            if (_selectedRange == HeatmapRange.AllTime) return false;

            DateTime today = DateTime.Today;

            if (_selectedRange == HeatmapRange.Month)
                return new DateTime(_currentDate.Year, _currentDate.Month, 1) < new DateTime(today.Year, today.Month, 1);

            return GetCurrentWeek().to < today;
        }

        public async Task LoadAsync(HeatmapRange range)
        {
            try
            {
                Log.Debug("HeatmapViewModel: Loading heatmap for range {Range}.", range);

                if (range == HeatmapRange.AllTime)
                {
                    await LoadYearAsync(_currentDate.Year);
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
                Log.Information("HeatmapViewModel: Loaded {Count} weeks for range {Range}.", Weeks.Count, range);

            }
            catch (AppException ex)
            {
                Log.Error(ex, "HeatmapViewModel: Failed to load heatmap for range {Range}.", range);
                await _messageService.ShowErrorAsync(ex);

            }
        }

        private async Task LoadYearAsync(int year)
        {
            try
            {
                Log.Debug("HeatmapViewModel: Loading heatmap data for year {Year}.", year);

                var from = new DateTime(year, 1, 1);
                var to = new DateTime(year, 12, 31);

                var cells = await _analyticsService.GetHeatmapCells(from, to);

                Weeks.Clear();
                SelectedCell = null;

                foreach (var chunk in cells.Chunk(7))
                    Weeks.Add(chunk.ToList());
            }
            catch (AppException ex)
            {
                Log.Error(ex, "HeatmapViewModel: Failed to load heatmap data for year {Year}.", year);
                await _messageService.ShowErrorAsync(ex);
            }   
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

            var start = firstDay.AddDays(-(((int)firstDay.DayOfWeek + 6) % 7));
            var end = lastDay.AddDays((7 - (int)lastDay.AddDays(1).DayOfWeek + 6) % 7);
            var lastMonday = lastDay.AddDays(-(((int)lastDay.DayOfWeek + 6) % 7));
            var finalEnd = lastMonday.AddDays(6);

            return (start, finalEnd);
        }

    }
}