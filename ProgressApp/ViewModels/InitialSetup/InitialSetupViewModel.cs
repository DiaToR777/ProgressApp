using ProgressApp.Localization;
using ProgressApp.Localization.Manager;
using ProgressApp.Localization.Models;
using ProgressApp.Services;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ProgressApp.ViewModels.InitialSetup
{
    public class InitialSetupViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settingsService;
        public List<LanguageModel> AvailableLanguages => LanguageConfig.AvailableLanguages;

        private string _username = string.Empty;
        private string _goal = string.Empty;
        private LanguageModel _selectedLanguage;

        public LanguageModel SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage == value) return;


                _selectedLanguage = value;
                OnPropertyChanged();

                if (_selectedLanguage != null)
                {
                    TranslationSource.Instance.CurrentCulture = new CultureInfo(_selectedLanguage.CultureCode);
                }
            }
        }


        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }
        public string Goal
        {
            get => _goal;
            set
            {
                _goal = value;
                OnPropertyChanged(nameof(Goal));
            }
        }

        public ICommand FinishCommand { get; }

        public Action? Completed { get; set; }

        public InitialSetupViewModel(SettingsService settings)
        {
            _settingsService = settings;
            SelectedLanguage = _settingsService.GetLanguage();
            FinishCommand = new RelayCommand(_ => Finish());
        }

        private void Finish()
        {
            try
            {
                _settingsService.SaveSettings(Username, Goal, AppTheme.Light, SelectedLanguage);
                TranslationSource.Instance.CurrentCulture = new CultureInfo(SelectedLanguage.CultureCode);
                Completed?.Invoke();
            }
            catch (ArgumentException ex) 
            {
                MessageBox.Show(ex.Message, "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
