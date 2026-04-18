using System.ComponentModel;

namespace ProgressApp.WpfUI.ViewModels.Analytics.Heatmap;

public enum HeatmapRange
{
    [Description("Range_Week")]
    Week,
    [Description("Range_Month")]
    Month,
    [Description("Range_AllTime")]
    AllTime
}
