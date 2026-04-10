using System.Windows;
using System.Windows.Input;

namespace ProgressApp.WpfUI.Helpers;

public static class PasswordBoxHelper
{
    public static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space) e.Handled = true;
    }

    public static void OnPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            var text = (string)e.DataObject.GetData(typeof(string));
            if (text != null && text.Contains(" ")) e.CancelCommand();
        }
    }

}
