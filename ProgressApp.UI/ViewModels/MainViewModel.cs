using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.WpfUI.ViewModels.InitialSetup;
using ProgressApp.WpfUI.ViewModels.Settings;
using ProgressApp.WpfUI.ViewModels.Table;
using ProgressApp.WpfUI.ViewModels.Today;
using Serilog;
using System.Windows.Input;

namespace ProgressApp.WpfUI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISettingsService _settingsService;

        private object? _currentView;
        private bool _isNavigationVisible = true;

        public object? CurrentView
        {
            get => _currentView;
            set
            {
                if (SetProperty(ref _currentView, value))
                {
                    Log.Information("Navigation: switched to {ViewModelName}", value?.GetType().Name ?? "null");
                }
            }
        }
        public bool IsNavigationVisible
        {
            get => _isNavigationVisible;
            set => SetProperty(ref _isNavigationVisible, value);
        }


        public ICommand ShowTodayCommand { get; }
        public ICommand ShowTableCommand { get; }
        public ICommand ShowSettingsCommand { get; }

        public MainViewModel(ISettingsService settings, IServiceProvider serviceProvider)
        {
            _settingsService = settings;
            _serviceProvider = serviceProvider;

            InitializeNavigationAsync();

            ShowTodayCommand = new RelayCommand(async _ => ShowToday());
            ShowTableCommand = new RelayCommand(async _ => ShowTable());
            ShowSettingsCommand = new RelayCommand(async _ => ShowSettings());

        }
        private async void InitializeNavigationAsync()
        {
            try
            {
                bool isFirstRun = await _settingsService.IsFirstRunAsync();
                IsNavigationVisible = false;

                if (isFirstRun)
                {
                    ShowInitialsSetup();
                }
                else
                {
                    ShowToday();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "MainViewModel: Failed to initialize navigation.");
            }
        }

        private void ShowInitialsSetup()
        {
            var vm = _serviceProvider.GetRequiredService<InitialSetupViewModel>();

            vm.Completed = () =>
            {
                IsNavigationVisible = true;
                ShowToday();
            };

            CurrentView = vm;
            IsNavigationVisible = false;
        }
        private void ShowTable()
        {
            CurrentView = _serviceProvider.GetRequiredService<TableViewModel>();
            IsNavigationVisible = true;
        }

        private void ShowToday()
        {
            CurrentView = _serviceProvider.GetRequiredService<TodayViewModel>();
            IsNavigationVisible = true;
        }

        private void ShowSettings()
        {
            CurrentView = _serviceProvider.GetRequiredService<SettingsViewModel>();
            IsNavigationVisible = true;
        }
    }
}
