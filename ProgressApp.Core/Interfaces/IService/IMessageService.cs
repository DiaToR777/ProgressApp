using ProgressApp.Core.Exceptions;

namespace ProgressApp.Core.Interfaces.IService
{
    public interface IMessageService
    {
        void ShowInfo(string messageKey);
        bool ShowConfirmation(string messageKey);
        void ShowError(AppException exception);
    }
}
