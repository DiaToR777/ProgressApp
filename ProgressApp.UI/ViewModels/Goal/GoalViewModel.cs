using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.WpfUI.Services;
using Serilog;

namespace ProgressApp.WpfUI.ViewModels.Goal
{
    public partial class GoalViewModel : ObservableObject
    {
        public Task Initialization { get; }

        private readonly IGoalService _goalService;
        private readonly IAnalyticsService _analyticsService;
        private readonly IMessageService _messageService;

        public MilestoneCompletionViewModel Completion { get; }
        public GoalActionsViewModel Actions { get; }

        [ObservableProperty] private Domain.Models.Goals.Goal? _activeGoal;
        [ObservableProperty] private MilestoneProgress? _milestoneProgress;

        [ObservableProperty] private bool _isEditingGoal;
        [ObservableProperty] private string _editGoalTitle = string.Empty;
        [ObservableProperty] private string _editGoalDescription = string.Empty;

        public bool IsNotEditingGoal => !IsEditingGoal;
        public bool HasActiveGoal => ActiveGoal != null;

        partial void OnIsEditingGoalChanged(bool value)
            => OnPropertyChanged(nameof(IsNotEditingGoal));

        partial void OnActiveGoalChanged(Domain.Models.Goals.Goal? value)
            => OnPropertyChanged(nameof(HasActiveGoal));

        public GoalViewModel(
            IGoalService goalService,
            IAnalyticsService analyticsService,
            IMessageService messageService,
            MilestoneCompletionViewModel completion,
            GoalActionsViewModel actions)
        {
            _goalService = goalService;
            _analyticsService = analyticsService;
            _messageService = messageService;
            Completion = completion;
            Actions = actions;

            Completion.Changed = LoadAsync;
            Actions.Changed = LoadAsync;

            Initialization = LoadAsync();
        }

        [RelayCommand]
        private void StartEditGoal()
        {
            if (ActiveGoal == null) return;

            EditGoalTitle = ActiveGoal.Title;
            EditGoalDescription = ActiveGoal.Description ?? string.Empty;
            IsEditingGoal = true;
        }

        [RelayCommand]
        private void CancelEditGoal() => IsEditingGoal = false;

        [RelayCommand]
        private async Task SaveGoal()
        {
            if (ActiveGoal == null || string.IsNullOrWhiteSpace(EditGoalTitle)) return;

            try
            {
                await _goalService.UpdateGoalAsync(
                    ActiveGoal.Id,
                    EditGoalTitle,
                    string.IsNullOrWhiteSpace(EditGoalDescription) ? null : EditGoalDescription);

                IsEditingGoal = false;
                await LoadAsync();
            }
            catch (AppException ex)
            {
                await _messageService.ShowErrorAsync(ex);
            }
        }

        private async Task LoadAsync()
        {
            try
            {
                ActiveGoal = await _goalService.GetActiveGoalAsync();
                MilestoneProgress = await _analyticsService.GetActiveMilestoneProgressAsync();

                Completion.Update(MilestoneProgress);
                Actions.Update(ActiveGoal);
            }
            catch (AppException ex)
            {
                Log.Error(ex, "GoalViewModel: Failed to load goal data");
                await _messageService.ShowErrorAsync(ex);
            }
        }
    }
}