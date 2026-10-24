using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.WpfUI.Services;

namespace ProgressApp.WpfUI.ViewModels.Goal
{
    public partial class MilestoneCompletionViewModel : ObservableObject
    {
        private readonly IGoalService _goalService;
        private readonly IMessageService _messageService;

        public Func<Task>? Changed { get; set; }

        [ObservableProperty] private MilestoneProgress? _progress;
        [ObservableProperty] private int _extendDays = 7;

        public bool IsMilestoneExpired => Progress?.IsExpired == true;
        public bool SuggestsSuccess => Progress?.SuggestedStatus == MilestoneStatus.Success;
        public bool SuggestsFail => Progress?.SuggestedStatus == MilestoneStatus.Fail;

        partial void OnProgressChanged(MilestoneProgress? value)
        {
            OnPropertyChanged(nameof(IsMilestoneExpired));
            OnPropertyChanged(nameof(SuggestsSuccess));
            OnPropertyChanged(nameof(SuggestsFail));
        }

        public MilestoneCompletionViewModel(IGoalService goalService, IMessageService messageService)
        {
            _goalService = goalService;
            _messageService = messageService;
        }

        public void Update(MilestoneProgress? progress) => Progress = progress;

        [RelayCommand]
        private Task CompleteSuccess() => CompleteAsync(MilestoneStatus.Success);

        [RelayCommand]
        private Task CompleteFail() => CompleteAsync(MilestoneStatus.Fail);

        private async Task CompleteAsync(MilestoneStatus status)
        {
            if (Progress == null) return;

            var confirmed = await _messageService.ShowConfirmationAsync("Msg_ConfirmCompleteMilestone");
            if (!confirmed) return;

            try
            {
                await _goalService.CompleteMilestoneAsync(Progress.MilestoneId, status);
                await NotifyChangedAsync();
            }
            catch (AppException ex)
            {
                await _messageService.ShowErrorAsync(ex);
            }
        }

        [RelayCommand]
        private async Task ExtendMilestone()
        {
            if (Progress == null) return;

            try
            {
                await _goalService.ExtendMilestoneAsync(Progress.MilestoneId, ExtendDays);
                await NotifyChangedAsync();
            }
            catch (AppException ex)
            {
                await _messageService.ShowErrorAsync(ex);
            }
        }

        private Task NotifyChangedAsync() => Changed?.Invoke() ?? Task.CompletedTask;
    }
}