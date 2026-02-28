using ProgressApp.Core.Services;
using ProgressApp.Core.Interfaces.IMessage;
using ProgressApp.Core.Models.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ProgressApp.Core.Models.Localization;
using ProgressApp.Core.Interfaces;

namespace ProgressApp.WpfUI.ViewModels.Settings
{
    public class SettingsViewModel : INotifyPropertyChanged
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
            set
            {
                if (_selectedLanguage == value) return;
                _selectedLanguage = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                var _sanitizedValue = value?.Length > 50 ? value.Substring(0, 50) : value;
                if (_username == _sanitizedValue) return;

                _username = _sanitizedValue;
                OnPropertyChanged();
            }
        }
        public string Goal
        {
            get => _goal;
            set
            {
                var _sanitizedValue = value?.Length > 50 ? value.Substring(0, 50) : value;
                if (_goal == _sanitizedValue) return;

                _goal = _sanitizedValue;
                OnPropertyChanged();
            }
        }

        public AppTheme SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (_selectedTheme == value) return;

                _selectedTheme = value;

                OnPropertyChanged();
            }
        }

        public Array AllThemes => Enum.GetValues(typeof(AppTheme));

        public ICommand SaveSettingsCommand { get; }
        public SettingsViewModel(ISettingsService settingsService, IMessageService messageService, ILocalizationService localizationService, IThemeService themeService)
        {
            _settingsService = settingsService;
            _messageService = messageService;

            Username = _settingsService.GetUserName();
            Goal = _settingsService.GetGoal();
            SelectedTheme = _settingsService.GetTheme();
            SelectedLanguage = _settingsService.GetLanguage();

            SaveSettingsCommand = new RelayCommand(
                    execute: _ =>
                    {
                        try
                        {
                            _settingsService.SaveSettings(Username, Goal, SelectedTheme, SelectedLanguage);
                            localizationService.ChangeLanguage(SelectedLanguage.CultureCode);
                            themeService.SetTheme(SelectedTheme);

                            _messageService.ShowInfo("Msg_SettingsSaved");
                        }
                        catch (Exception ex)
                        {
                            _messageService.ShowError(ex.Message);
                        }
                    },
                    canExecute: _ => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Goal)
                );
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Goal));
            OnPropertyChanged(nameof(SelectedTheme));
            OnPropertyChanged(nameof(AvailableLanguages));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

