using CommunityToolkit.Mvvm.ComponentModel;
using ProgressApp.Domain.Models.Goals;

namespace ProgressApp.WpfUI.ViewModels.Today;

public partial class ActionCheckboxViewModel : ObservableObject
{
    public Guid ActionId { get; }
    public string Title { get; }

    [ObservableProperty]
    private bool _isCompleted;

    public ActionCheckboxViewModel(GoalAction action)
    {
        ActionId = action.Id;
        Title = action.Title;
    }
}