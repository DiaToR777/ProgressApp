using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Localization;
using ProgressApp.Infrastructure.Configuration;
using ProgressApp.WpfUI.Localization;
using ProgressApp.WpfUI.Localization.Managers;
using ProgressApp.WpfUI.Services;
using ProgressApp.WpfUI.Themes;
using Serilog;

namespace ProgressApp.WpfUI.ViewModels.InitialSetup
{
    public partial class InitialSetupViewModel : ObservableObject
    {
        private readonly IOnboardingService _onboardingService;
        private readonly IMessageService _messageService;
        private readonly ILocalizationService _localizationService;
        private readonly IAuthService _authService;
        private readonly IAppConfigService _appConfigService;

        public List<LanguageModel> AvailableLanguages => LanguageConfig.AvailableLanguages;

        public ObservableCollection<ActionInputViewModel> Actions { get; } = new();

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
        private string _username = string.Empty;

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
        private string _goal = string.Empty;

        [ObservableProperty] private string _goalDescription = string.Empty;

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
        private int _milestoneDays = 40;

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
        private string _password = string.Empty;

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
        private string _confirmPassword = string.Empty;

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
        private bool _isBusy;

        [ObservableProperty] private LanguageModel _selectedLanguage;

        partial void OnSelectedLanguageChanged(LanguageModel value)
        {
            if (value != null)
            {
                Log.Debug("InitialSetup: User selected language: {Culture}", value.CultureCode);
                _localizationService.ChangeLanguage(value.CultureCode);
            }
        }

        public Action? Completed { get; set; } //TODO Event

        public InitialSetupViewModel(
            IOnboardingService onboardingService,
            IAppConfigService appConfigService,
            ILocalizationService localizationService,
            IMessageService messageService,
            IAuthService authService)
        {
            _onboardingService = onboardingService;
            _appConfigService = appConfigService;
            _localizationService = localizationService;
            _messageService = messageService;
            _authService = authService;

            _selectedLanguage = LanguageConfig.AvailableLanguages.First();
        }

        [RelayCommand]
        private void AddAction()
        {
            Actions.Add(new ActionInputViewModel());
        }

        [RelayCommand]
        private void RemoveAction(ActionInputViewModel action)
        {
            Actions.Remove(action);
        }

        [RelayCommand(CanExecute = nameof(CanFinish), AllowConcurrentExecutions = false)]
        private async Task FinishAsync()
        {
            try
            {
                IsBusy = true;

                bool isRegistered = await Task.Run(() => _authService.RegisterAsync(Password));
                if (!isRegistered) return;

                Log.Information("InitialSetup: Registration success");

                var actionsData = Actions
                    .Where(a => !string.IsNullOrWhiteSpace(a.Title))
                    .Select(a => (a.Title, a.TargetCountPerWeek))
                    .ToList();

                await _onboardingService.CompleteOnboardingAsync(
                    Username,
                    Goal,
                    string.IsNullOrWhiteSpace(GoalDescription) ? null : GoalDescription,
                    MilestoneDays,
                    actionsData);

                var config = new AppConfig
                {
                    Language = SelectedLanguage.CultureCode,
                    Theme = AppTheme.Light.ToString()
                };
                
                _appConfigService.Save(config);

                Log.Information("InitialSetupVM: Setup saved. Invoking completion.");
                Completed?.Invoke();
            }
            catch (AppException ex)
            {
                Log.Error(ex, "InitialSetup: Critical error during setup finish");
                await _messageService.ShowErrorAsync(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanFinish()
        {
            return !IsBusy &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Goal) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   MilestoneDays > 0 &&
                   Password == ConfirmPassword;
        }
    }
}