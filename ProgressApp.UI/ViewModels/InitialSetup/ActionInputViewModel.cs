using CommunityToolkit.Mvvm.ComponentModel;

namespace ProgressApp.WpfUI.ViewModels.InitialSetup;

public partial class ActionInputViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private int _targetCountPerWeek = 7;
}