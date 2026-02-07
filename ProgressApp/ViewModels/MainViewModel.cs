using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Services;
using ProgressApp.ViewModels.InitialSetup;
using ProgressApp.ViewModels.Settings;
using ProgressApp.ViewModels.Table;
using ProgressApp.ViewModels.Today;
using ProgressApp.Views.InitialSetup;
using ProgressApp.Views.Settings;
using ProgressApp.Views.Table;
using ProgressApp.Views.Today;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ProgressApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SettingsService _settingsService;

        private object? _currentView;
        private bool _isNavigationVisible = true;

        public object? CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        private void ShowTable()
        {
            var vm = _serviceProvider.GetRequiredService<TableViewModel>();

            CurrentView = new TableView { DataContext = vm };

            IsNavigationVisible = true;
        }
        public bool IsNavigationVisible
        {
            get => _isNavigationVisible;
            set
            {
                _isNavigationVisible = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowTodayCommand { get; }
        public ICommand ShowTableCommand { get; }
        public ICommand ShowSettingsCommand { get; }

        public MainViewModel(SettingsService settings, IServiceProvider serviceProvider)
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

            CurrentView = new InitialSetupView { DataContext = vm };
            IsNavigationVisible = false;


            OnPropertyChanged(nameof(CurrentView));
            OnPropertyChanged(nameof(IsNavigationVisible));

        }
        private void ShowToday()
        {
            var vm = _serviceProvider.GetRequiredService<TodayViewModel>();
            CurrentView = new TodayView { DataContext = vm };
        }

        private void ShowSettings()
        {
            var vm = _serviceProvider.GetRequiredService<SettingsViewModel>();
            CurrentView = new SettingsView { DataContext = vm };
            IsNavigationVisible = true;
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
