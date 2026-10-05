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
        
        public List<GoalAction> CurrentActions => ActiveGoal?.Milestones.FirstOrDefault()?.Actions ?? new();
        
        private readonly IGoalService _goalService;
        private readonly ICheckinService _checkinService;
        private readonly IMessageService _messageService;
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotEditingGoal))]
        private bool _isEditingGoal;

        [ObservableProperty]
        private string _editGoalTitle = string.Empty;

        [ObservableProperty]
        private string _editGoalDescription = string.Empty;
        
        [ObservableProperty]
        private GoalAction? _editingAction;
        
        [ObservableProperty]
        private string _newActionTitle = string.Empty;

        [ObservableProperty]
        private int _newActionTargetCountPerWeek = 7;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentActions))]
        private Domain.Models.Goals.Goal? _activeGoal;
        
        public bool IsNotEditingGoal => !IsEditingGoal;

        [ObservableProperty] 
        private MilestoneProgress? _milestoneProgress;

        public GoalViewModel(
            IGoalService goalService,
            ICheckinService checkinService,
            IMessageService messageService)
        {
            _goalService = goalService;
            _checkinService = checkinService;
            _messageService = messageService;

            Initialization = LoadAsync();
        }
        
        [RelayCommand]
        private void StartEditAction(GoalAction action)
        {
            EditingAction = action;
            NewActionTitle = action.Title;
            NewActionTargetCountPerWeek = action.TargetCountPerWeek;
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
        private void CancelEditGoal()
        {
            IsEditingGoal = false;
        }

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

        [RelayCommand]
        private async Task SaveAction()
        {
            if (string.IsNullOrWhiteSpace(NewActionTitle)) return;

            try
            {
                if (EditingAction != null)
                {
                    await _goalService.UpdateActionAsync(EditingAction.Id, NewActionTitle, NewActionTargetCountPerWeek);
                }
                else
                {
                    var milestoneId = ActiveGoal?.Milestones.FirstOrDefault()?.Id;
                    if (milestoneId == null) return;

                    await _goalService.AddActionAsync(milestoneId.Value, NewActionTitle, NewActionTargetCountPerWeek);
                }
                
                CancelEditAction();
            
                await LoadAsync();
            }
            catch (AppException ex)
            {
                await _messageService.ShowErrorAsync(ex);
            }
        }

        [RelayCommand]
        private async Task RemoveAction(GoalAction action)
        {
            var confirmed = await _messageService.ShowConfirmationAsync("Msg_ConfirmRemoveAction", action.Title);
            if (!confirmed) return;

            try
            {
                await _goalService.RemoveActionAsync(action.Id);
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
                MilestoneProgress = await _checkinService.GetActiveMilestoneProgressAsync();
            }
            catch (AppException ex)
            {
                Log.Error(ex, "GoalViewModel: Failed to load goal data");
                await _messageService.ShowErrorAsync(ex);
            }
        }
        
        [RelayCommand]
        private void CancelEditAction()
        {
            EditingAction = null;
            NewActionTitle = string.Empty;
            NewActionTargetCountPerWeek = 7;
        }
    }
}