using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.WpfUI.Localization.Helpers;
using ProgressApp.WpfUI.ViewModels.Analytics.Enums;
using ProgressApp.WpfUI.ViewModels.Analytics.Heatmap;
using ProgressApp.WpfUI.ViewModels.Analytics.Table;
using Serilog;

namespace ProgressApp.WpfUI.ViewModels.Analytics
{
    public partial class AnalyticsViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private object? _currentAnalyticsView;

        [ObservableProperty]
        private AnalyticsMode _selectedViewOption;

        public IEnumerable<LocalizedEnum<AnalyticsMode>> ViewOptions { get; }

        public AnalyticsViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            ViewOptions = Enum.GetValues(typeof(AnalyticsMode))
                      .Cast<AnalyticsMode>()
                      .Select(m => new LocalizedEnum<AnalyticsMode>(m))
                      .ToList();

            _selectedViewOption = AnalyticsMode.Table;

            _ = UpdateViewCommand.ExecuteAsync(AnalyticsMode.Table);
        }   
        partial void OnSelectedViewOptionChanged(AnalyticsMode value)
        {
            _ = UpdateViewCommand.ExecuteAsync(value);
        }

        [RelayCommand]
        internal async Task UpdateViewAsync(AnalyticsMode option)
        {
            try
            {
                switch (option)
                {
                    case AnalyticsMode.Table:
                        CurrentAnalyticsView = _serviceProvider.GetRequiredService<TableViewModel>();
                        break;

                    case AnalyticsMode.Heatmap:
                        var heatmapVm = _serviceProvider.GetRequiredService<HeatmapViewModel>();
                        await heatmapVm.LoadAsync(HeatmapRange.Week);
                        CurrentAnalyticsView = heatmapVm;
                        break;
                }

                Log.Information("AnalyticsViewModel: switched to {Mode}", option);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AnalyticsViewModel: Failed to switch to {Option}.", option);
            }
        }
    }
}