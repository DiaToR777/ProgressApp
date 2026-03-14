using ProgressApp.Core.Interfaces;
using ProgressApp.Core.Interfaces.IMessage;
using ProgressApp.Core.Models.Enums;
using ProgressApp.Core.Models.Localization;
using ProgressApp.Core.Services;
using Serilog;
using System.Windows.Input;

namespace ProgressApp.WpfUI.ViewModels.Settings
{
    public class SettingsViewModel : ViewModelBase
    {
        public List<LanguageModel> AvailableLanguages => LanguageConfig.AvailableLanguages;

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

            try
            {
                Username = _settingsService.GetUserName();
                Goal = _settingsService.GetGoal();
                SelectedTheme = _settingsService.GetTheme();
                SelectedLanguage = _settingsService.GetLanguage();
                Log.Debug("SettingsVM: Current settings loaded for user {Username}", Username);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SettingsVM: Failed to load initial settings");
            }

            SaveSettingsCommand = new RelayCommand(
                execute: _ =>
                {
                    try
                    {
                        Log.Information("SettingsVM: Saving settings. New Culture: {Culture}, Theme: {Theme}",
                            SelectedLanguage.CultureCode, SelectedTheme);

                        _settingsService.SaveSettings(Username, Goal, SelectedTheme, SelectedLanguage);
                        localizationService.ChangeLanguage(SelectedLanguage.CultureCode);
                        themeService.SetTheme(SelectedTheme);

                        _messageService.ShowInfo("Msg_SettingsSaved");
                        Log.Information("SettingsVM: Settings updated successfully.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "SettingsVM: Error saving settings");
                        _messageService.ShowError("Msg_ErrorSavingSettings");
                    }
                },
                canExecute: _ => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Goal)
            );
        }
    }
}

