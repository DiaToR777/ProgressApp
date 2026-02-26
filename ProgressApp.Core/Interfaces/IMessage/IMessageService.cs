namespace ProgressApp.Core.Interfaces.IMessage
{
    public interface IMessageService
    {
        void ShowInfo(string messageKey);
        bool ShowConfirmation(string messageKey);
        void ShowError(string messageKey);
    }
}
