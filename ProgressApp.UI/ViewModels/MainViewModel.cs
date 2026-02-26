using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Core.Services;
using ProgressApp.WpfUI.ViewModels.Today;
using ProgressApp.WpfUI.ViewModels;
using ProgressApp.WpfUI.ViewModels.InitialSetup;
using ProgressApp.WpfUI.ViewModels.Settings;
using ProgressApp.WpfUI.ViewModels.Table;
using ProgressApp.WpfUI.Views.InitialSetup;
using ProgressApp.WpfUI.Views.Settings;
using ProgressApp.WpfUI.Views.Table;
using ProgressApp.WpfUI.Views.Today;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ProgressApp.WpfUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
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
                _currentView = value;
                OnPropertyChanged();
            }
        }

        private void ShowTable()
        {
            var vm = _serviceProvider.GetRequiredService<ViewModels.Table.TableViewModel>();

            CurrentView = new Views.Table.TableView { DataContext = vm };

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
            var vm = _serviceProvider.GetRequiredService<InitialSetup.InitialSetupViewModel>();
            vm.Completed = () =>
            {
                IsNavigationVisible = true;
                ShowToday();
            };

            CurrentView = new Views.InitialSetup.InitialSetupView { DataContext = vm };
            IsNavigationVisible = false;


            OnPropertyChanged(nameof(CurrentView));
            OnPropertyChanged(nameof(IsNavigationVisible));

        }
        private void ShowToday()
        {
            var vm = _serviceProvider.GetRequiredService<TodayViewModel>();
            CurrentView = new Views.Today.TodayView { DataContext = vm };
        }

        private void ShowSettings()
        {
            var vm = _serviceProvider.GetRequiredService<ViewModels.Settings.SettingsViewModel>();
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
