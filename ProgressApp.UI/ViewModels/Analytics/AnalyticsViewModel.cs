using Microsoft.Extensions.DependencyInjection;
using ProgressApp.WpfUI.Localization.Helpers;
using ProgressApp.WpfUI.ViewModels.Analytics.Enums;
using ProgressApp.WpfUI.ViewModels.Analytics.Heatmap;
using ProgressApp.WpfUI.ViewModels.Analytics.Table;

namespace ProgressApp.WpfUI.ViewModels.Analytics
{
    public class AnalyticsViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private object? _currentAnalyticsView;
        public IEnumerable<LocalizedEnum<AnalyticsMode>> ViewOptions { get; }

        private AnalyticsMode _selectedMode;

        public object? CurrentAnalyticsView
        {
            get => _currentAnalyticsView;
            set => SetProperty(ref _currentAnalyticsView, value);
        }

        public AnalyticsMode SelectedViewOption
        {
            get => _selectedMode;
            set
            {
                if (SetProperty(ref _selectedMode, value))
                {
                    UpdateView(value);
                }
            }
        }
        //TODO add logging and error handling
        public AnalyticsViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            ViewOptions = Enum.GetValues(typeof(AnalyticsMode))
                      .Cast<AnalyticsMode>()
                      .Select(m => new LocalizedEnum<AnalyticsMode>(m))
                      .ToList();

            SelectedViewOption = AnalyticsMode.Table;
            UpdateView(AnalyticsMode.Table);
        }

        private async void UpdateView(AnalyticsMode option)
        {
            switch (option)
            {
                case AnalyticsMode.Table:
                    var tableVm = _serviceProvider.GetRequiredService<TableViewModel>();

                    CurrentAnalyticsView = tableVm;
                    break;

                case AnalyticsMode.Heatmap:
                    var heatmapVm = _serviceProvider.GetRequiredService<HeatmapViewModel>();

                    await heatmapVm.LoadAsync(HeatmapRange.Week); //TODO

                    CurrentAnalyticsView = heatmapVm;
                    break;
            }
        }
    }
}
