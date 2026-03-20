using ProgressApp.Core.Models.Enums;
using ProgressApp.Core.Models.Localization;
using Serilog;
using System.Windows.Input;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;

namespace ProgressApp.WpfUI.ViewModels.InitialSetup
{
    public class InitialSetupViewModel : ViewModelBase
    {

        private readonly ISettingsService _settingsService;
        private readonly IMessageService _messageService;
        private readonly ILocalizationService _localizationService;

        public List<LanguageModel> AvailableLanguages => LanguageConfig.AvailableLanguages;

        private string _username = string.Empty;
        private string _goal = string.Empty;
        private LanguageModel _selectedLanguage;

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

        public InitialSetupViewModel(ISettingsService settings, ILocalizationService localizationService, IMessageService messageService)
        {
            _messageService = messageService;
            _localizationService = localizationService;
            _settingsService = settings;
            SelectedLanguage = _settingsService.GetLanguage();

            FinishCommand = new RelayCommand(
                execute: _ =>
                {
                    try
                    {
                        Finish();
                    }
                    catch (AppException ex)
                    {
                        Log.Error(ex, "InitialSetup: Critical error during setup finish");
                        _messageService.ShowError(ex);
                    }
                },
                canExecute: _ => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Goal)
              );
        }

        private void Finish()
        {
            Log.Information("InitialSetup: Attempting to save setup. Username: {Username}, Goal: {Goal}, Lang: {Lang}",
        Username, Goal, SelectedLanguage?.CultureCode);

            _settingsService.SaveSettings(Username, Goal, AppTheme.Light, SelectedLanguage);
            Completed?.Invoke();
            Log.Information("InitialSetup: Setup saved successfully. Invoking completion.");
        }
    }
}
