using ProgressApp.Localization.Manager;
using System.Windows;

namespace ProgressApp.Services.Message
{
    public class MessageService : IMessageService
    {
        public MessageService() { }
        public void ShowInfo(string messageKey)
        {
            string message = GetLocalizedText(messageKey);
            string title = GetLocalizedText("Title_Information");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public bool ShowConfirmation(string messageKey)
        {
            string message = GetLocalizedText(messageKey);
            string title = GetLocalizedText("Title_Confirmation");
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
        public void ShowError(string exMessage)
        {
            string title = GetLocalizedText("Title_Error");
            MessageBox.Show(exMessage, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private string GetLocalizedText(string key)
        {
            return TranslationSource.Instance[key] ?? key;
        }
    }
}
