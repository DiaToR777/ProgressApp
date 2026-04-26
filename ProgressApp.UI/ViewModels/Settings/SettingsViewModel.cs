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

        private readonly ISettingsService _settingsService;
        private readonly IMessageService _messageService;
        private readonly IAppConfigService _appConfigService;
        private readonly IDataExchangeService _dataExchangeService;
        private readonly ILocalizationService _localizationService;
        private readonly IAppThemeService _themeService;
        private readonly IAuthService _authService;

        private string _username = string.Empty;
        private string _goal = string.Empty;
        private AppTheme _selectedTheme;
        private LanguageModel _selectedLanguage;

        private string _newDbPassword = string.Empty;
        private string _confirmDbPassword = string.Empty;
        private bool _isChangingPassword;
        private bool _isDbEncrypted;
        public bool IsDbEncrypted
        {
            get => _isDbEncrypted;
            set => SetProperty(ref _isDbEncrypted, value);
        }

        public bool IsChangingPassword
        {
            get => _isChangingPassword;
            set
            {
                if (SetProperty(ref _isChangingPassword, value))
                    OnPropertyChanged(nameof(IsNotChangingPassword));
            }
        }

        public bool IsNotChangingPassword => !IsChangingPassword;

        public string NewDbPassword
        {
            get => _newDbPassword;
            set => SetProperty(ref _newDbPassword, value);

        }

        public string ConfirmDbPassword
        {
            get => _confirmDbPassword;
            set => SetProperty(ref _confirmDbPassword, value);
        }


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
        public ICommand ExportEntriesCommand { get; }
        public ICommand ImportEntriesCommand { get; }
        public ICommand TogglePasswordFieldsCommand { get; }
        public ICommand ApplyNewPasswordCommand { get; }
        public ICommand RemovePasswordCommand { get; }

        public SettingsViewModel(
            ISettingsService settingsService,
            IAppConfigService appConfigService,
            IMessageService messageService,
            ILocalizationService localizationService,
            IAppThemeService themeService,
            IDataExchangeService dataExchangeService,
            IAuthService authService)
        {
            _authService = authService;
            _dataExchangeService = dataExchangeService;
            _appConfigService = appConfigService;
            _settingsService = settingsService;
            _messageService = messageService;
            _localizationService = localizationService;
            _themeService = themeService;

            Initialize();

            SaveSettingsCommand = new RelayCommand(
                executeAsync: async _ => { await SaveSettings(); },
                canExecute: _ => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Goal));

            ExportEntriesCommand = new RelayCommand(async _ => await ExportEntriesAsync());

            ImportEntriesCommand = new RelayCommand(executeAsync: async _ => await ImportEntriesAsync());

            ApplyNewPasswordCommand = new RelayCommand(
                executeAsync: async _ => await ApplyNewPassword(),
                canExecute: _ => !string.IsNullOrWhiteSpace(NewDbPassword) && NewDbPassword == ConfirmDbPassword
            );

            TogglePasswordFieldsCommand = new RelayCommand(_ => IsChangingPassword = !IsChangingPassword);

            RemovePasswordCommand = new RelayCommand(
                executeAsync: async _ => await RemovePasswordAsync(),
                canExecute: _ => IsDbEncrypted);
        }

        private async Task RemovePasswordAsync()
        {
            var confirmed = await _messageService.ShowConfirmationAsync("Msg_RemovePasswordConfirmation");
            if (!confirmed) return;
            try
            {
                await _authService.RemovePasswordAsync();
                IsDbEncrypted = false;
                Log.Debug("SettingsVM: IsDbEncrypted = {Value}", IsDbEncrypted);
                await _messageService.ShowInfoAsync("Msg_PasswordRemovedSuccess");
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Error removing password");
                await _messageService.ShowErrorAsync(ex);
            }
        }
        private async Task ApplyNewPassword()
        {
            try
            {
                if (IsDbEncrypted)
                {
                    Log.Information("SettingsVM: Changing DB password to a new one.");

                    await _authService.ChangePasswordAsync(NewDbPassword);

                    await _messageService.ShowInfoAsync("Msg_PasswordChangedSuccess");

                    NewDbPassword = string.Empty;
                    ConfirmDbPassword = string.Empty;
                    IsChangingPassword = false;
                }
                else
                {
                    Log.Information("SettingsVM: Setting new DB password for unencrypted database.");
                    await _authService.SetPasswordAsync(NewDbPassword);
                    IsDbEncrypted = true;

                    Log.Debug("SettingsVM: IsDbEncrypted = {Value}", IsDbEncrypted);
                    await _messageService.ShowInfoAsync("Msg_PasswordSetSuccess");

                    NewDbPassword = string.Empty;
                    ConfirmDbPassword = string.Empty;
                    IsChangingPassword = false;
                }
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Password change failed");
                await _messageService.ShowErrorAsync(ex);
            }
        }

        private async Task ImportEntriesAsync()
        {
            Log.Information("SettingsVM: Starting import process... Getting file path from user.");

            var path = _messageService.OpenFileDialog("CSV files (*.csv)|*.csv");

            if (string.IsNullOrEmpty(path))
            {
                Log.Information("SettingsVM: Import cancelled by user.");
                return;
            }

            var confirmed = await _messageService.ShowConfirmationAsync("Msg_ImportConfirmation");

            if (!confirmed) return;

            try
            {
                var importedCount = await _dataExchangeService.ImportFromCsvAsync(path);
                await _messageService.ShowInfoAsync("Msg_SuccessImportInfo", importedCount);
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Import failed");
                await _messageService.ShowErrorAsync(ex);
            }
        }
        private async Task ExportEntriesAsync()
        {
            try
            {
                Log.Information("SettingsVM: Starting export process... Getting file path from user.");
                var path = _messageService.SaveFileDialog(
                    $"Progress_Backup_{DateTime.Now:yyyyMMdd}",
                    "CSV files (*.csv)|*.csv"
                );

                if (string.IsNullOrEmpty(path))
                {
                    Log.Information("SettingsVM: Export cancelled by user.");
                    return;
                }

                Log.Information("SettingsVM: Exporting data to {FilePath}", path);
                await _dataExchangeService.ExportToCsvAsync(path);

                await _messageService.ShowInfoAsync("Msg_SuccessExportInfo");
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Error exporting entries");
                await _messageService.ShowErrorAsync(ex);
            }
        }
        private async Task SaveSettings()
        {
            try
            {
                Log.Information("SettingsVM: Saving settings. New Culture: {Culture}, Theme: {Theme}",
                    SelectedLanguage.CultureCode, SelectedTheme);

                var config = _appConfigService.Load();

                config.Theme = SelectedTheme.ToString();
                config.Language = SelectedLanguage.CultureCode;
                config.Username = Username;
                _appConfigService.Save(config);

                await _settingsService.SaveGoalAsync(Goal);
                _localizationService.ChangeLanguage(SelectedLanguage.CultureCode);
                _themeService.SetTheme(SelectedTheme);

                await _messageService.ShowInfoAsync("Msg_SettingsSaved");
                Log.Information("SettingsVM: Settings updated successfully.");
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Error saving settings");
                await _messageService.ShowErrorAsync(ex);
            }
        }
        private async void Initialize()
        {
            try
            {
                IsDbEncrypted = await _authService.GetDbStatusAsync() == DbStatus.Encrypted;

                var goalTask = _settingsService.GetGoalAsync();
                var config = _appConfigService.Load();

                Username = config.Username;
                Goal = await goalTask;
                SelectedTheme = Enum.Parse<AppTheme>(config.Theme);
                SelectedLanguage = LanguageConfig.GetByCode(config.Language);

                Log.Debug("SettingsVM: All settings loaded in parallel");
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Error while getting settings");
                await _messageService.ShowErrorAsync(ex);
            }
        }

    }
}

