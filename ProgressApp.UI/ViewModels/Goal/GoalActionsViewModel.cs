using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.WpfUI.Services;

namespace ProgressApp.WpfUI.ViewModels.Goal
{
    public partial class GoalActionsViewModel : ObservableObject
    {
        private readonly IGoalService _goalService;
        private readonly IMessageService _messageService;
        private Guid? _milestoneId;

        public Func<Task>? Changed { get; set; }

        [ObservableProperty] private List<GoalAction> _items = new();
        [ObservableProperty] private GoalAction? _editingAction;
        [ObservableProperty] private string _newActionTitle = string.Empty;
        [ObservableProperty] private int _newActionTargetCountPerWeek = 7;

        public bool IsEditingAction => EditingAction != null;

        partial void OnEditingActionChanged(GoalAction? value)
            => OnPropertyChanged(nameof(IsEditingAction));

        public GoalActionsViewModel(IGoalService goalService, IMessageService messageService)
        {
            _goalService = goalService;
            _messageService = messageService;
        }

        public void Update(Domain.Models.Goals.Goal? goal)
        {
            var milestone = goal?.Milestones.FirstOrDefault();
            _milestoneId = milestone?.Id;
            Items = milestone?.Actions ?? new List<GoalAction>();
        }

        [RelayCommand]
        private void StartEditAction(GoalAction action)
        {
            EditingAction = action;
            NewActionTitle = action.Title;
            NewActionTargetCountPerWeek = action.TargetCountPerWeek;
        }

        [RelayCommand]
        private void CancelEditAction() => ResetForm();

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
                    if (_milestoneId == null) return;
                    await _goalService.AddActionAsync(_milestoneId.Value, NewActionTitle, NewActionTargetCountPerWeek);
                }

                ResetForm();
                await NotifyChangedAsync();
            }
            catch (AppException ex)
            {
                await _messageService.ShowErrorAsync(ex);
            }
        }

        [RelayCommand]
        private async Task RemoveAction(GoalAction action)
        {
            var confirmed = await _messageService.ShowConfirmationAsync("Msg_ConfirmRemoveAction");
            if (!confirmed) return;

            try
            {
                await _goalService.RemoveActionAsync(action.Id);
                await NotifyChangedAsync();
            }
            catch (AppException ex)
            {
                await _messageService.ShowErrorAsync(ex);
            }
        }

        private void ResetForm()
        {
            EditingAction = null;
            NewActionTitle = string.Empty;
            NewActionTargetCountPerWeek = 7;
        }

        private Task NotifyChangedAsync() => Changed?.Invoke() ?? Task.CompletedTask;
    }
}