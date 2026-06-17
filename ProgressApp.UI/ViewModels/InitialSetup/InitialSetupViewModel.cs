using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Config;
using ProgressApp.Core.Models.Enums;
using ProgressApp.Core.Models.Localization;
using Serilog;

namespace ProgressApp.WpfUI.ViewModels.InitialSetup
{
    public partial class InitialSetupViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly IMessageService _messageService;
        private readonly ILocalizationService _localizationService;
        private readonly IAuthService _authService;
        private readonly IAppConfigService _appConfigService;

        public List<LanguageModel> AvailableLanguages => LanguageConfig.AvailableLanguages;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
        private string _username = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
        private string _goal = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
        private string _password = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(FinishCommand))]
        private bool _isBusy;

        [ObservableProperty]
        private LanguageModel _selectedLanguage;

        partial void OnSelectedLanguageChanged(LanguageModel value)
        {
            if (value != null)
            {
                Log.Debug("InitialSetup: User selected language: {Culture}", value.CultureCode);
                _localizationService.ChangeLanguage(value.CultureCode);
            }
        }

        public Action? Completed { get; set; }

        public InitialSetupViewModel(
            ISettingsService settings,
            IAppConfigService appConfigService,
            ILocalizationService localizationService,
            IMessageService messageService,
            IAuthService authService)
        {
            _settingsService = settings;
            _appConfigService = appConfigService;
            _localizationService = localizationService;
            _messageService = messageService;
            _authService = authService;

            _selectedLanguage = LanguageConfig.AvailableLanguages.First();
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

                await _settingsService.SaveGoalAsync(Goal);

                var config = new AppConfig
                {
                    Language = SelectedLanguage.CultureCode,
                    Username = Username,
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
                   Password == ConfirmPassword;
        }
    }
}