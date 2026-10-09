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
        private readonly IMessageService _messageService;
        private readonly IAppConfigService _appConfigService;
        private readonly ILocalizationService _localizationService;
        private readonly IAppThemeService _themeService;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public Task Initialization { get; }
        public List<LanguageModel> AvailableLanguages => LanguageConfig.AvailableLanguages;
        public Array AllThemes => Enum.GetValues(typeof(AppTheme));

        [ObservableProperty] private bool _isDbEncrypted;

        [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsNotChangingPassword))]
        private bool _isChangingPassword;

        public bool IsNotChangingPassword => !IsChangingPassword;

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ApplyNewPasswordCommand))]
        private string _newDbPassword = string.Empty;

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(ApplyNewPasswordCommand))]
        private string _confirmDbPassword = string.Empty;

        [ObservableProperty] 
        private LanguageModel _selectedLanguage;

        [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand))]
        private string _username = string.Empty;

        partial void OnUsernameChanged(string value)
        {
            if (value?.Length > 50) Username = value.Substring(0, 50);
        }

        [ObservableProperty] 
        private AppTheme _selectedTheme;

        public SettingsViewModel(
            IAppConfigService appConfigService,
            IMessageService messageService,
            ILocalizationService localizationService,
            IAppThemeService themeService,
            IAuthService authService,
            IUserService userService)
        {
            _authService = authService;
            _userService = userService;
            _appConfigService = appConfigService;
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

                var config = _appConfigService.Load();

                var user = await _userService.GetCurrentUserAsync();
                Username = user?.Username ?? string.Empty;
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
                await _userService.UpdateUsernameAsync(Username);
                _appConfigService.Save(config);


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

        private bool CanSaveSettings() => !string.IsNullOrWhiteSpace(Username) && Username.Length <= 50 &&
                                          !Username.Any(char.IsWhiteSpace);

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

                await _messageService.ShowInfoAsync(wasEncrypted
                    ? "Msg_PasswordChangedSuccess"
                    : "Msg_PasswordSetSuccess");

                NewDbPassword = ConfirmDbPassword = string.Empty;
                IsChangingPassword = false;
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Password operation failed");
                await _messageService.ShowErrorAsync(ex);
            }
        }

        private bool CanApplyNewPassword() =>
            !string.IsNullOrWhiteSpace(NewDbPassword) && NewDbPassword == ConfirmDbPassword;

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