using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Heatmap;
using ProgressApp.WpfUI.Localization.Helpers;
using ProgressApp.WpfUI.Localization.Managers;
using Serilog;
using System.Collections.ObjectModel;

namespace ProgressApp.WpfUI.ViewModels.Analytics.Heatmap
{
    public partial class HeatmapViewModel : ObservableObject
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IMessageService _messageService;

        private DateOnly? _firstEntryDate;
        private DateTime _currentDate = DateTime.Today;

        public string[] DayLabels => CultureHelper.GetAbbreviatedDayNames();
        public ObservableCollection<List<DayCell>> Weeks { get; } = new();
        public IEnumerable<LocalizedEnum<HeatmapRange>> Ranges { get; }

        [ObservableProperty]
        private DayCell? _selectedCell;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CellSize))]
        [NotifyPropertyChangedFor(nameof(CellMargin))]
        [NotifyPropertyChangedFor(nameof(ShowNavigation))]
        [NotifyPropertyChangedFor(nameof(PeriodTitle))]
        private HeatmapRange _selectedRange = HeatmapRange.Week;

        partial void OnSelectedRangeChanged(HeatmapRange value)
        {
            _ = LoadAsync(value);
        }

        public string PeriodTitle => SelectedRange switch
        {
            HeatmapRange.Week => $"{GetCurrentWeek().from.ToString("dd MMM", TranslationSource.Instance.CurrentCulture)} — {GetCurrentWeek().to.ToString("dd MMM yyyy", TranslationSource.Instance.CurrentCulture)}",
            HeatmapRange.Month => _currentDate.ToString("MMMM yyyy", TranslationSource.Instance.CurrentCulture),
            HeatmapRange.AllTime => string.Empty,
            _ => string.Empty
        };

        public bool ShowNavigation => SelectedRange switch
        {
            HeatmapRange.AllTime => _firstEntryDate.HasValue && _firstEntryDate.Value.Year < DateTime.Today.Year,
            _ => true
        };

        public int CellSize => SelectedRange switch
        {
            HeatmapRange.Week => 40,
            HeatmapRange.Month => 30,
            HeatmapRange.AllTime => 13,
            _ => 30
        };

        public int CellMargin => SelectedRange switch
        {
            HeatmapRange.Week => 4,
            HeatmapRange.Month => 3,
            HeatmapRange.AllTime => 1,
            _ => 3
        };

        public HeatmapViewModel(IAnalyticsService analyticsService, IMessageService messageService)
        {
            _analyticsService = analyticsService;
            _messageService = messageService;

            Ranges = Enum.GetValues(typeof(HeatmapRange))
                .Cast<HeatmapRange>()
                .Select(r => new LocalizedEnum<HeatmapRange>(r))
                .ToList();

            _ = InitializeAsync();
        }

        internal async Task InitializeAsync()
        {
            await GetFirstEntryDate();
            await LoadAsync(SelectedRange);
            RefreshUI();
        }

        [RelayCommand]
        private void SelectCell(object? param)
        {
            if (param is DayCell cell)
            {
                SelectedCell = cell;
            }
            else if (param == null)
            {
                SelectedCell = null;
            }
        }
        [RelayCommand(CanExecute = nameof(CanGoBack))]
        private async Task PreviousPeriod()
        {
            _currentDate = SelectedRange switch
            {
                HeatmapRange.Week => _currentDate.AddDays(-7),
                HeatmapRange.Month => _currentDate.AddMonths(-1),
                HeatmapRange.AllTime => _currentDate.AddYears(-1),
                _ => _currentDate
            };
            RefreshUI();
            await LoadAsync(SelectedRange);
        }

        [RelayCommand(CanExecute = nameof(CanGoForward))]
        private async Task NextPeriod()
        {
            _currentDate = SelectedRange switch
            {
                HeatmapRange.Week => _currentDate.AddDays(7),
                HeatmapRange.Month => _currentDate.AddMonths(1),
                HeatmapRange.AllTime => _currentDate.AddYears(1),
                _ => _currentDate
            };
            RefreshUI();
            await LoadAsync(SelectedRange);
        }

        private void RefreshUI()
        {
            OnPropertyChanged(nameof(PeriodTitle));
            OnPropertyChanged(nameof(ShowNavigation));

            PreviousPeriodCommand.NotifyCanExecuteChanged();
            NextPeriodCommand.NotifyCanExecuteChanged();
        }

        private async Task GetFirstEntryDate()
        {
            try
            {
                var firstDate = await _analyticsService.GetFirstEntryDateAsync();
                _firstEntryDate = firstDate.HasValue ? DateOnly.FromDateTime(firstDate.Value) : null;
            }
            catch (AppException ex)
            {
                Log.Error(ex, "Heatmap: Error fetching first date");
            }
        }

        public async Task LoadAsync(HeatmapRange range)
        {
            try
            {
                if (range == HeatmapRange.AllTime)
                {
                    await LoadYearAsync(_currentDate.Year);
                    return;
                }

                var (from, to) = range == HeatmapRange.Week ? GetCurrentWeek() : GetCurrentMonthAligned();
                var allCells = await Task.Run(() => _analyticsService.GetHeatmapCells(from, to)) ?? new List<DayCell>();

                var lookup = allCells.ToDictionary(c => c.Date.Date);

                Weeks.Clear();
                SelectedCell = null;

                var current = from.Date;
                while (current <= to.Date)
                {
                    var week = new List<DayCell>();
                    for (int i = 0; i < 7 && current <= to.Date; i++)
                    {
                        week.Add(lookup.TryGetValue(current, out var cell) ? cell : new DayCell { Date = current });
                        current = current.AddDays(1);
                    }
                    Weeks.Add(week);
                }
            }
            catch (AppException ex)
            {
                Log.Error(ex, "Heatmap: Load failed");
                await _messageService.ShowErrorAsync(ex);
            }
        }

        private async Task LoadYearAsync(int year)
        {
            var cells = await Task.Run(() => _analyticsService.GetHeatmapCells(new DateTime(year, 1, 1), new DateTime(year, 12, 31))) 
                ?? new List<DayCell>();
            Weeks.Clear();
            SelectedCell = null;
            foreach (var chunk in cells.Chunk(7))
                Weeks.Add(chunk.ToList());
        }

        private bool CanGoBack()
        {
            if (!_firstEntryDate.HasValue) return false;
            if (SelectedRange == HeatmapRange.AllTime) return _currentDate.Year > _firstEntryDate.Value.Year;

            DateOnly firstDate = _firstEntryDate.Value;
            DateOnly currentViewStart = SelectedRange == HeatmapRange.Week
                ? DateOnly.FromDateTime(GetCurrentWeek().from)
                : new DateOnly(_currentDate.Year, _currentDate.Month, 1);

            return currentViewStart > firstDate;
        }

        private bool CanGoForward()
        {
            if (SelectedRange == HeatmapRange.AllTime) return false;
            if (SelectedRange == HeatmapRange.Month)
                return new DateTime(_currentDate.Year, _currentDate.Month, 1) < new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            return GetCurrentWeek().to < DateTime.Today;
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
            var lastMonday = lastDay.AddDays(-(((int)lastDay.DayOfWeek + 6) % 7));
            return (start, lastMonday.AddDays(6));
        }
    }
}