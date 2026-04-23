using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.WpfUI.Localization.Managers;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace ProgressApp.WpfUI.Services.Message
{
    public class MessageService : IMessageService
    {
        public async Task ShowInfoAsync(string messageKey, params object[] args)
        {
            string message = GetFormattedText(messageKey, args);
            string title = GetLocalizedText("Title_Information");

            var uiMessageBox = CreateMessageBox(title, message, SymbolRegular.Info24, "SystemAccentColorBrush");
            await uiMessageBox.ShowDialogAsync();
        }

        public async Task ShowErrorAsync(AppException ex)
        {
            string message = GetFormattedText(ex.ResourceKey, ex.Args);

            string title = GetLocalizedText("Title_Error");

            var uiMessageBox = CreateMessageBox(title, message, SymbolRegular.ErrorCircle24, "SystemFillColorCriticalBrush");

            uiMessageBox.SecondaryButtonText = GetLocalizedText("Btn_OpenLogs");
            uiMessageBox.SecondaryButtonAppearance = ControlAppearance.Transparent;

            var result = await uiMessageBox.ShowDialogAsync();

            if (result == Wpf.Ui.Controls.MessageBoxResult.Secondary)
                OpenLogsFolder(); 
        }

        private void OpenLogsFolder()
        {
            try
            {
                string logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ProgressApp",
                    "logs"); 

                if (Directory.Exists(logPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", logPath);
                }
            }
            catch { /* Ignored */ }
        }

        public async Task<bool> ShowConfirmationAsync(string messageKey, params object[] args)
        {
            string message = GetFormattedText(messageKey, args);
            string title = GetLocalizedText("Title_Confirmation");

            var uiMessageBox = CreateMessageBox(title, message, SymbolRegular.QuestionCircle24, "SystemAccentColorBrush");

            uiMessageBox.PrimaryButtonText = GetLocalizedText("Btn_Yes");
            uiMessageBox.CloseButtonText = GetLocalizedText("Btn_No");

            var result = await uiMessageBox.ShowDialogAsync();
            return result == Wpf.Ui.Controls.MessageBoxResult.Primary;
        }

        public async Task ShowErrorIncorrectPasswordAsync()
        {
            string message = GetLocalizedText("Error_IncorrectPassword");
            string title = GetLocalizedText("Title_Error");

            var uiMessageBox = CreateMessageBox(title, message, SymbolRegular.Password24, "SystemFillColorCriticalBrush");
            await uiMessageBox.ShowDialogAsync();
        }

        private Wpf.Ui.Controls.MessageBox CreateMessageBox(string title, string message, SymbolRegular icon, string brushKey)
        {
            return new Wpf.Ui.Controls.MessageBox
            {
                Title = title,
                CloseButtonText = GetLocalizedText("Btn_Ok"),
                MaxWidth = 500,
                Owner = Application.Current.MainWindow,
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new SymbolIcon
                        {
                            Symbol = icon,
                            FontSize = 24,
                            Margin = new Thickness(0, 0, 15, 0),
                            Foreground = Application.Current.Resources[brushKey] as Brush ?? Brushes.Gray
                        },
                        new Wpf.Ui.Controls.TextBlock
                        {
                            Text = message,
                            VerticalAlignment = VerticalAlignment.Center,
                            TextWrapping = TextWrapping.Wrap,
                            MaxWidth = 400
                        }
                    }
                }
            };
        }

        private string GetFormattedText(string key, object[] args)
        {
            string pattern = GetLocalizedText(key);
            return (args != null && args.Length > 0) ? string.Format(pattern, args) : pattern;
        }

        private string GetLocalizedText(string key)
        {
            return TranslationSource.Instance[key] ?? key;
        }

        public string? OpenFileDialog(string filter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = filter };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? SaveFileDialog(string defaultName, string filter)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog { FileName = defaultName, Filter = filter };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}