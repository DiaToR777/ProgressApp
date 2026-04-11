using ProgressApp.WpfUI.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProgressApp.WpfUI.Views.Login;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }
    private void PasswordBox_PreviewKeyDown(object sender, KeyEventArgs e)
=> PasswordBoxHelper.OnPreviewKeyDown(sender, e);

    private void PasswordBox_Pasting(object sender, DataObjectPastingEventArgs e)
        => PasswordBoxHelper.OnPasting(sender, e);

}
