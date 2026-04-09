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
        public ICommand ExportEntriesCommand { get; }
        public ICommand ImportEntriesCommand { get; }

        public SettingsViewModel(
            ISettingsService settingsService,
            IAppConfigService appConfigService,
            IMessageService messageService,
            ILocalizationService localizationService,
            IAppThemeService themeService,
            IDataExchangeService dataExchangeService)
        {
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

            var confirmed = _messageService.ShowConfirmation("Msg_ImportConfirmation");

            if (!confirmed) return;

            try
            {
                var importedCount = await _dataExchangeService.ImportFromCsvAsync(path);
                _messageService.ShowInfo("Msg_SuccessImportInfo", importedCount);
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Import failed");
                _messageService.ShowError(ex);
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

                _messageService.ShowInfo("Msg_SuccessExportInfo");
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Error exporting entries");
                _messageService.ShowError(ex);
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

                _messageService.ShowInfo("Msg_SettingsSaved");
                Log.Information("SettingsVM: Settings updated successfully.");
            }
            catch (AppException ex)
            {
                Log.Error(ex, "SettingsVM: Error saving settings");
                _messageService.ShowError(ex);
            }

        }
        private async void Initialize()
        {
            try
            {
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
                _messageService.ShowError(ex);
            }
        }
    }
}

