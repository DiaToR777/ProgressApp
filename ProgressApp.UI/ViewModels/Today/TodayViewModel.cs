using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Journal;
using ProgressApp.WpfUI.Localization.Helpers;
using ProgressApp.WpfUI.Services;
using Serilog;

namespace ProgressApp.WpfUI.ViewModels.Today
{
    public partial class TodayViewModel : ObservableObject
    {
        public Task Initialization { get; }
        
        public ObservableCollection<ActionCheckboxViewModel> ActiveActions { get; } = new();

        private readonly ICheckinService _checkinService;
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

        public TodayViewModel(ICheckinService checkinService, IMessageService messageService, IAnalyticsService analyticsService)
        {
            _checkinService = checkinService;
            _messageService = messageService;
            _analyticsService = analyticsService;

            AllResult = Enum.GetValues(typeof(DayResult))
                            .Cast<DayResult>()
                            .Select(r => new LocalizedEnum<DayResult>(r))
                            .ToList();

            Initialization = InitializeAsync();
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task Save()
        {
            try
            {
                var completions = ActiveActions.ToDictionary(a => a.ActionId, a => a.IsCompleted);
                await _checkinService.SaveTodayAsync(Description, SelectedResult, completions);

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

                var entry = await _checkinService.GetTodayAsync();
                var actions = await _checkinService.GetActiveActionsAsync();

                ActiveActions.Clear();
                foreach (var action in actions)
                {
                    var vm = new ActionCheckboxViewModel(action);

                    var existingLog = entry?.ActionLogs.FirstOrDefault(l => l.GoalActionId == action.Id);
                    if (existingLog != null)
                        vm.IsCompleted = existingLog.IsCompleted; // восстанавливаем состояние чекбокса

                    ActiveActions.Add(vm);
                }
                
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


