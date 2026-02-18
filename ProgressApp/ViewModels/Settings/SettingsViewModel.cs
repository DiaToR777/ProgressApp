using ProgressApp.Localization;
using ProgressApp.Localization.Manager;
using ProgressApp.Localization.Models;
using ProgressApp.Services;
using ProgressApp.Services.Message;
using ProgressApp.Themes.Managers;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ProgressApp.ViewModels.Settings
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public List<LanguageModel> AvailableLanguages => LanguageConfig.AvailableLanguages;

        private readonly SettingsService _settingsService;
        private readonly IMessageService _messageService;

        private string _username;
        private string _goal;
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
            set { _username = value; OnPropertyChanged(); }
        }
        public string Goal
        {
            get => _goal;
            set { _goal = value; OnPropertyChanged(); }
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
        public SettingsViewModel(SettingsService settingsService, IMessageService messageService)
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
                            ThemeManager.ApplyTheme(SelectedTheme);
                            TranslationSource.Instance.CurrentCulture = new CultureInfo(SelectedLanguage.CultureCode);

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

