using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Enums;
using ProgressApp.Core.Models.Localization;
using Serilog;
using System.Windows.Input;

namespace ProgressApp.WpfUI.ViewModels.Settings
{
    public class SettingsViewModel : ViewModelBase
    {
        public List<LanguageModel> AvailableLanguages => LanguageConfig.AvailableLanguages;

        public bool IsBusy { get; set; } = false;

        private readonly ISettingsService _settingsService;
        private readonly IMessageService _messageService;

        private string _username = string.Empty;
        private string _goal = string.Empty;
        private AppTheme _selectedTheme;
        private LanguageModel _selectedLanguage;

        public LanguageModel SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
        }

        public string Username
        {
            get => _username;
            set
            {
                var _sanitizedValue = value?.Length > 50 ? value.Substring(0, 50) : value;
                SetProperty(ref _username, _sanitizedValue);
            }
        }
        public string Goal
        {
            get => _goal;
            set
            {
                var _sanitizedValue = value?.Length > 150 ? value.Substring(0, 150) : value;
                SetProperty(ref _goal, _sanitizedValue);
            }
        }

        public AppTheme SelectedTheme
        {
            get => _selectedTheme;
            set => SetProperty(ref _selectedTheme, value);
        }

        public Array AllThemes => Enum.GetValues(typeof(AppTheme));

        public ICommand SaveSettingsCommand { get; }
        public SettingsViewModel(ISettingsService settingsService, IMessageService messageService, ILocalizationService localizationService, IThemeService themeService)
        {
            _settingsService = settingsService;
            _messageService = messageService;

            Initialize();

            try
            {
                Log.Debug("SettingsVM: Current settings loaded for user {Username}", Username);
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Failed to load initial settings");
                messageService.ShowError(ex);
            }

            SaveSettingsCommand = new RelayCommand(
                executeAsync:async _ =>
                {
                    try
                    {
                        Log.Information("SettingsVM: Saving settings. New Culture: {Culture}, Theme: {Theme}",
                            SelectedLanguage.CultureCode, SelectedTheme);

                        await _settingsService.SaveSettingsAsync(Username, Goal, SelectedTheme, SelectedLanguage);
                        localizationService.ChangeLanguage(SelectedLanguage.CultureCode);
                        themeService.SetTheme(SelectedTheme);

                        _messageService.ShowInfo("Msg_SettingsSaved");
                        Log.Information("SettingsVM: Settings updated successfully.");
                    }
                    catch (AppException ex)
                    {
                        Log.Error(ex, "SettingsVM: Error saving settings");
                        _messageService.ShowError(ex);
                    }
                },
                canExecute: _ => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Goal)
            );
        }

        private async void Initialize()
        {
             Username = await _settingsService.GetUserNameAsync();
             Goal = await _settingsService.GetGoalAsync();
            SelectedTheme = await _settingsService.GetThemeAsync();
            SelectedLanguage = await _settingsService.GetLanguageAsync();

        }
    }
}

