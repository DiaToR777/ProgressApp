using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.WpfUI.Localization.Managers;
using System.Windows;

namespace ProgressApp.WpfUI.Services.Message
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
        public void ShowError(AppException ex)
        {
            string pattern = GetLocalizedText(ex.ResourceKey);

            string message = (ex.Args != null && ex.Args.Length > 0)
                ? string.Format(pattern, ex.Args)
                : pattern;

            if (ex.InnerException != null)
                message += $"\n\nDetails: {ex.InnerException.Message}";

            string title = GetLocalizedText("Title_Error");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private string GetLocalizedText(string key) 
        {
            return TranslationSource.Instance[key] ?? key;
        }
    }
}
