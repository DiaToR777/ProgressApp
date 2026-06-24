using ProgressApp.Domain.Exceptions;

namespace ProgressApp.WpfUI.Services
{
    public interface IMessageService
    {
        Task ShowInfoAsync(string messageKey, params object[] args);
        Task<bool> ShowConfirmationAsync(string messageKey, params object[] args);
        Task ShowErrorAsync(AppException ex);
        Task ShowErrorIncorrectPasswordAsync();
        string? SaveFileDialog(string defaultName, string filter);
        string? OpenFileDialog(string filter);
    }
}
