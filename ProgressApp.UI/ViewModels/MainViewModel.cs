using Microsoft.Extensions.DependencyInjection;
using ProgressApp.WpfUI.ViewModels.Today;
using ProgressApp.WpfUI.Views.Today;
using ProgressApp.WpfUI.Views.InitialSetup;
using ProgressApp.WpfUI.ViewModels.InitialSetup;
using ProgressApp.WpfUI.Views.Settings;
using ProgressApp.WpfUI.ViewModels.Settings;
using ProgressApp.Core.Services;
using ProgressApp.WpfUI.ViewModels.Table;
using ProgressApp.WpfUI.Views.Table;
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
            set => SetProperty(ref _currentView, value);
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

            if (_settingsService.IsFirstRun())
            {
                ShowInitialsSetup();
            }
            else
            {
                ShowToday();
            }


            ShowTodayCommand = new RelayCommand(_ => ShowToday());
            ShowTableCommand = new RelayCommand(_ => ShowTable());
            ShowSettingsCommand = new RelayCommand(_ => ShowSettings());

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
        }

        private void ShowSettings()
        {
            CurrentView = _serviceProvider.GetRequiredService<SettingsViewModel>();
            IsNavigationVisible = true;
        }
    }
}
