using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Journal;
using ProgressApp.WpfUI.Localization.Helpers;
using Serilog;

namespace ProgressApp.WpfUI.ViewModels.Today
{
    public partial class TodayViewModel : ObservableObject
    {
        private readonly IJournalService _journalService;
        private readonly IMessageService _messageService;
        private readonly IAnalyticsService _analyticsService;

        public DateOnly CurrentDate { get; } = DateOnly.FromDateTime(DateTime.Now);
        public IEnumerable<LocalizedEnum<DayResult>> AllResult { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string _description = string.Empty;

        partial void OnDescriptionChanging(string? value)
        {
            if (value?.Length > 1500)
                _description = value.Substring(0, 1500);
        }

        [ObservableProperty]
        private DayResult _selectedResult;

        [ObservableProperty]
        private int _currentStreak;

        public TodayViewModel(IJournalService journalService, IMessageService messageService, IAnalyticsService analyticsService)
        {
            _journalService = journalService;
            _messageService = messageService;
            _analyticsService = analyticsService;

            AllResult = Enum.GetValues(typeof(DayResult))
                            .Cast<DayResult>()
                            .Select(r => new LocalizedEnum<DayResult>(r))
                            .ToList();

            _ = InitializeAsync();
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task Save()
        {
            try
            {
                await _journalService.SaveTodayAsync(Description, SelectedResult);

                CurrentStreak = await _analyticsService.GetCurrentStreakAsync();

                await _messageService.ShowInfoAsync("Msg_RecordSaved");
                Log.Debug("TodayViewModel: Entry saved successfully");
            }
            catch (AppException ex)
            {
                Log.Error(ex, "TodayViewModel: Failed to save entry");
                await _messageService.ShowErrorAsync(ex);
            }
        }

        private bool CanSave() => !string.IsNullOrWhiteSpace(Description);

        private async Task InitializeAsync()
        {
            try
            {
                Log.Debug("TodayViewModel: Loading data for {Date}", CurrentDate);

                var entry = await _journalService.GetTodayAsync();

                CurrentStreak = await _analyticsService.GetCurrentStreakAsync();

                if (entry != null)
                {
                    Description = entry.Description ;
                    SelectedResult = entry.Result;
                }
                else
                {
                    SelectedResult = DayResult.PartialSuccess;
                }
            }
            catch (AppException ex)
            {
                Log.Error(ex, "TodayViewModel: Error loading today's entry");
                await _messageService.ShowErrorAsync(ex);
            }
        }
    }
}


