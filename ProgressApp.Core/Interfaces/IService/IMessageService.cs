using ProgressApp.Core.Exceptions;

namespace ProgressApp.Core.Interfaces.IService
{
    public interface IMessageService
    {
        void ShowInfo(string messageKey, params object[] args);
        bool ShowConfirmation(string messageKey, params object[] args);
        void ShowError(AppException exception);
        void ShowErrorIncorrectPassword();
        string? SaveFileDialog(string defaultName, string filter);
        string? OpenFileDialog(string filter);
    }
}
