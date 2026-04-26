using ProgressApp.Core.Models.Journal;
using ProgressApp.WpfUI.Localization.Helpers;
using Serilog;
using System.Windows.Input;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;

namespace ProgressApp.WpfUI.ViewModels.Today
{
    public class TodayViewModel : ViewModelBase
    {
        private readonly IJournalService _journalService;
        private readonly IMessageService _messageService;
        private readonly IAnalyticsService _analyticsService;

        private string _description = string.Empty;
        private DayResult _selectedResult;
        public DateOnly CurrentDate { get; } = DateOnly.FromDateTime(DateTime.Now);
        public IEnumerable<LocalizedEnum<DayResult>> AllResult { get; }

        private int _currentStreak;
        public int CurrentStreak
        {
            get => _currentStreak;
            set => SetProperty(ref _currentStreak, value);
        }
        public string Description
        {
            get => _description;
            set
            {
                var sanitizedValue = value?.Length > 1500 ? value.Substring(0, 1500) : value;
                SetProperty(ref _description, sanitizedValue);
            }
        }
        public DayResult SelectedResult
        {
            get => _selectedResult;
            set => SetProperty(ref _selectedResult, value);

        }
        public ICommand SaveCommand { get; }

        public TodayViewModel(IJournalService journalService, IMessageService messageService, IAnalyticsService analyticsService)
        {
            _journalService = journalService;
            _messageService = messageService;
            _analyticsService = analyticsService;

            AllResult = Enum.GetValues(typeof(DayResult))
                            .Cast<DayResult>()
                            .Select(r => new LocalizedEnum<DayResult>(r))
                            .ToList();

            SaveCommand = new RelayCommand(
                    executeAsync: async _ =>
                    {
                        try
                        {
                            await _journalService.SaveTodayAsync(Description, SelectedResult);
                            CurrentStreak = await _analyticsService.GetCurrentStreakAsync();

                            await _messageService.ShowInfoAsync("Msg_RecordSaved");
                            Log.Debug("TodayViewModel: UI Success notification shown to user");
                        }
                        catch (AppException ex)
                        {
                            Log.Error(ex, "TodayViewModel: Failed to save entry");
                            await _messageService.ShowErrorAsync(ex);
                        }
                    },
                    canExecute: _ => !string.IsNullOrWhiteSpace(Description)
                );
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                await LoadTodayAsync();
            }
            catch (AppException ex)
            {
                Log.Error(ex, "TodayViewModel: Error loading today's entry");
                await _messageService.ShowErrorAsync(ex);
            }
        }

        private async Task LoadTodayAsync()
        {
            Log.Debug("TodayViewModel: Loading data for {Date}", CurrentDate);
            var entry = await _journalService.GetTodayAsync();

            CurrentStreak = await _analyticsService.GetCurrentStreakAsync();
            if (entry != null)
            {
                Description = entry.Description;
                SelectedResult = entry.Result;
            }
            else
            {
                SelectedResult = DayResult.PartialSuccess;
            }
        }

    }
}
