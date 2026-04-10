using ProgressApp.WpfUI.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProgressApp.WpfUI.Views.InitialSetup;

public partial class InitialSetupView : UserControl
{
    public InitialSetupView()
    {
        InitializeComponent();
    }
    private void PasswordBox_PreviewKeyDown(object sender, KeyEventArgs e)
=> PasswordBoxHelper.OnPreviewKeyDown(sender, e);

    private void PasswordBox_Pasting(object sender, DataObjectPastingEventArgs e)
        => PasswordBoxHelper.OnPasting(sender, e);

}
