using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Enums;
using ProgressApp.WpfUI.ViewModels.Analytics;
using ProgressApp.WpfUI.ViewModels.InitialSetup;
using ProgressApp.WpfUI.ViewModels.Login;
using ProgressApp.WpfUI.ViewModels.Settings;
using ProgressApp.WpfUI.ViewModels.Today;
using Serilog;

namespace ProgressApp.WpfUI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private bool _isNavigationVisible = true;

        partial void OnCurrentViewChanged(object? value)
        {
            Log.Information("Navigation: switched to {ViewModelName}", value?.GetType().Name ?? "null");
        }

        public MainViewModel(IAuthService authSevice, IServiceProvider serviceProvider)
        {
            _authService = authSevice;
            _serviceProvider = serviceProvider;

            _ = InitializeNavigationAsync();
        }

        private async Task InitializeNavigationAsync()
        {
            try
            {
                var status = await _authService.GetDbStatusAsync();

                switch (status)
                {
                    case DbStatus.NotCreated: ShowInitialsSetup(); break;
                    case DbStatus.Encrypted: ShowLogin(); break;
                    case DbStatus.Unencrypted: ShowToday(); break;
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "MainViewModel: Failed to initialize navigation.");
            }
        }

        [RelayCommand]
        private void ShowToday()
        {
            CurrentView = _serviceProvider.GetRequiredService<TodayViewModel>();
            IsNavigationVisible = true;
        }

        [RelayCommand]
        private void ShowAnalytics()
        {
            CurrentView = _serviceProvider.GetRequiredService<AnalyticsViewModel>();
            IsNavigationVisible = true;
        }

        [RelayCommand]
        private void ShowSettings()
        {
            CurrentView = _serviceProvider.GetRequiredService<SettingsViewModel>();
            IsNavigationVisible = true;
        }

        private void ShowLogin()
        {
            var vm = _serviceProvider.GetRequiredService<LoginViewModel>();
            vm.Completed = () =>
            {
                IsNavigationVisible = true;
                ShowToday();
            };

            CurrentView = vm;
            IsNavigationVisible = false;
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
    }
}