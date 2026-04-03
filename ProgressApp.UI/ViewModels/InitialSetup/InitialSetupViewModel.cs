using ProgressApp.Core.Models.Enums;
using ProgressApp.Core.Models.Localization;
using Serilog;
using System.Windows.Input;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Config;

namespace ProgressApp.WpfUI.ViewModels.InitialSetup
{
    public class InitialSetupViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IMessageService _messageService;
        private readonly ILocalizationService _localizationService;
        private readonly IAuthService _authService;
        private readonly IAppConfigService _appConfigService;


        public List<LanguageModel> AvailableLanguages => LanguageConfig.AvailableLanguages;

        private string _username = string.Empty;
        private string _goal = string.Empty;
        private LanguageModel _selectedLanguage;

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public LanguageModel SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (SetProperty(ref _selectedLanguage, value))
                {
                    if (_selectedLanguage != null)
                    {
                        Log.Debug("InitialSetup: User selected language: {Culture}", _selectedLanguage.CultureCode);
                        _localizationService.ChangeLanguage(_selectedLanguage.CultureCode);
                    }
                }
            }
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }
        public string Goal
        {
            get => _goal;
            set => SetProperty(ref _goal, value);
        }

        public ICommand FinishCommand { get; }

        public Action? Completed { get; set; }

        public InitialSetupViewModel(ISettingsService settings, IAppConfigService appConfigService, ILocalizationService localizationService, IMessageService messageService, IAuthService authService)
        {
            _messageService = messageService;
            _localizationService = localizationService;
            _settingsService = settings;
            _authService = authService;
            _appConfigService = appConfigService;

            SelectedLanguage = LanguageConfig.AvailableLanguages.First();

            FinishCommand = new RelayCommand(
                executeAsync: async _ =>
                {
                    if (IsBusy) return;
                    try
                    {
                        IsBusy = true;
                        await Task.Run(async () =>
                        { 
                            await FinishAsync();
                        });
                    }
                    catch (AppException ex)
                    {
                        Log.Error(ex, "InitialSetup: Critical error during setup finish");
                        _messageService.ShowError(ex);
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                },
                canExecute: _ => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Goal) && !string.IsNullOrWhiteSpace(Password) && !IsBusy
              );
        }

        private async Task FinishAsync()
        {
            bool isRegistered = await _authService.RegisterAsync(Password);
            if (isRegistered)
            {
                Log.Information("InitialSetup: Registration success during setup finish");

                await _settingsService.SaveGoalAsync(Goal);

                AppConfig config = new AppConfig
                {
                    Language = SelectedLanguage.CultureCode,
                    Username = Username,
                    Theme = AppTheme.Light.ToString()
                };
                _appConfigService.Save(config);

                Log.Information("InitialSetupVM: Setup saved successfully. Invoking completion.");

                Completed?.Invoke();
                return;
            }
         }
    }
}
