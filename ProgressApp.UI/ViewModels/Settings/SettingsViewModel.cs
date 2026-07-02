using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Enums;
using ProgressApp.Domain.Models.Localization;
using ProgressApp.Infrastructure.Configuration;
using ProgressApp.WpfUI.Localization;
using ProgressApp.WpfUI.Localization.Managers;
using ProgressApp.WpfUI.Services;
using ProgressApp.WpfUI.Themes;
using Serilog;

namespace ProgressApp.WpfUI.ViewModels.Settings
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly IMessageService _messageService;
        private readonly IAppConfigService _appConfigService;
        private readonly IDataExchangeService _dataExchangeService;
        private readonly ILocalizationService _localizationService;
        private readonly IAppThemeService _themeService;
        private readonly IAuthService _authService;


        public Task Initialization { get; }
        public List<LanguageModel> AvailableLanguages => LanguageConfig.AvailableLanguages;
        public Array AllThemes => Enum.GetValues(typeof(AppTheme));

        [ObservableProperty]
        private bool _isDbEncrypted;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotChangingPassword))]
        private bool _isChangingPassword;

        public bool IsNotChangingPassword => !IsChangingPassword;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplyNewPasswordCommand))]
        private string _newDbPassword = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplyNewPasswordCommand))]
        private string _confirmDbPassword = string.Empty;

        [ObservableProperty]
        private LanguageModel _selectedLanguage;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private string _username = string.Empty;

        partial void OnUsernameChanged(string value)
        {
            if (value?.Length > 50) Username = value.Substring(0, 50);
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private string _goal = string.Empty;

        partial void OnGoalChanged(string value)
        {
            if (value?.Length > 150) Goal = value.Substring(0, 150);
        }

        [ObservableProperty]
        private AppTheme _selectedTheme;

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

            Initialization = InitializeAsync();
        }

        private async Task InitializeAsync()
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

                Log.Debug("SettingsVM: All settings loaded");
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Error loading settings");
                await _messageService.ShowErrorAsync(ex);
            }
        }

        [RelayCommand(CanExecute = nameof(CanSaveSettings))]
        private async Task SaveSettings()
        {
            try
            {
                var config = _appConfigService.Load();
                config.Theme = SelectedTheme.ToString();
                config.Language = SelectedLanguage.CultureCode;
                config.Username = Username;

                _appConfigService.Save(config);
                await _settingsService.SaveGoalAsync(Goal);

                _localizationService.ChangeLanguage(SelectedLanguage.CultureCode);
                _themeService.SetTheme(SelectedTheme);

                await _messageService.ShowInfoAsync("Msg_SettingsSaved");
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Error saving settings");
                await _messageService.ShowErrorAsync(ex);
            }
        }

        private bool CanSaveSettings() => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Goal);

        [RelayCommand]
        private async Task ExportEntries()
        {
            try
            {
                var path = _messageService.SaveFileDialog($"Progress_Backup_{DateTime.Now:yyyyMMdd}", "CSV files (*.csv)|*.csv");
                if (string.IsNullOrEmpty(path)) return;

                await _dataExchangeService.ExportToCsvAsync(path);
                await _messageService.ShowInfoAsync("Msg_SuccessExportInfo");
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Export failed");
                await _messageService.ShowErrorAsync(ex);
            }
        }

        [RelayCommand]
        private async Task ImportEntries()
        {
            var path = _messageService.OpenFileDialog("CSV files (*.csv)|*.csv");
            if (string.IsNullOrEmpty(path)) return;

            if (!await _messageService.ShowConfirmationAsync("Msg_ImportConfirmation")) return;

            try
            {
                var count = await _dataExchangeService.ImportFromCsvAsync(path);
                await _messageService.ShowInfoAsync("Msg_SuccessImportInfo", count);
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Import failed");
                await _messageService.ShowErrorAsync(ex);
            }
        }

        [RelayCommand]
        private void TogglePasswordFields() => IsChangingPassword = !IsChangingPassword;

        [RelayCommand(CanExecute = nameof(CanApplyNewPassword))]
        private async Task ApplyNewPassword()
        {
            try
            {
                bool wasEncrypted = IsDbEncrypted;

                if (!wasEncrypted)
                    await _authService.SetPasswordAsync(NewDbPassword);

                else
                    await _authService.ChangePasswordAsync(NewDbPassword);

                IsDbEncrypted = true;

                await _messageService.ShowInfoAsync(wasEncrypted ? "Msg_PasswordChangedSuccess" : "Msg_PasswordSetSuccess");

                NewDbPassword = ConfirmDbPassword = string.Empty;
                IsChangingPassword = false;
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Password operation failed");
                await _messageService.ShowErrorAsync(ex);
            }
        }

        private bool CanApplyNewPassword() => !string.IsNullOrWhiteSpace(NewDbPassword) && NewDbPassword == ConfirmDbPassword;

        [RelayCommand(CanExecute = nameof(IsDbEncrypted))]
        private async Task RemovePassword()
        {
            if (!await _messageService.ShowConfirmationAsync("Msg_RemovePasswordConfirmation")) return;

            try
            {
                await _authService.RemovePasswordAsync();
                IsDbEncrypted = false;
                await _messageService.ShowInfoAsync("Msg_PasswordRemovedSuccess");
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Error removing password");
                await _messageService.ShowErrorAsync(ex);
            }
        }
    }
}