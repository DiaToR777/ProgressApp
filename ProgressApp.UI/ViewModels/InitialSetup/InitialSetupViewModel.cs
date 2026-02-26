using ProgressApp.Core.Interfaces;
using ProgressApp.Core.Interfaces.IMessage;
using ProgressApp.Core.Models.Enums;
using ProgressApp.Core.Models.Localization;
using ProgressApp.Core.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ProgressApp.WpfUI.ViewModels.InitialSetup
{
    public class InitialSetupViewModel : INotifyPropertyChanged
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
                if (_selectedLanguage == value) return;


                _selectedLanguage = value;
                OnPropertyChanged();

                if (_selectedLanguage != null)
                {
                    _localizationService.ChangeLanguage(SelectedLanguage.CultureCode);
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

        public InitialSetupViewModel(ISettingsService settings, ILocalizationService localizationService)
        {
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
                    catch (Exception ex)
                    {
                        _messageService.ShowError(ex.Message);
                    }
                },
                canExecute: _ => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Goal)
              );
        }

        private void Finish()
        {
            try
            {
                _settingsService.SaveSettings(Username, Goal, AppTheme.Light, SelectedLanguage);
                _localizationService.ChangeLanguage(SelectedLanguage.CultureCode);
                Completed?.Invoke();
            }
            catch (ArgumentException ex)
            {
                _messageService.ShowError(ex.Message);
                return;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
