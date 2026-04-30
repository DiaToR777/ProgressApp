using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using Serilog;

namespace ProgressApp.WpfUI.ViewModels.Login
{
    public partial class LoginViewModel : ObservableObject
    {
        public Action? Completed { get; set; } //TOOO Event

        private readonly IAuthService _authService;
        private readonly IMessageService _messageService;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _password = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private bool _isBusy;

        public LoginViewModel(IAuthService authService, IMessageService messageService)
        {
            _authService = authService;
            _messageService = messageService;
        }

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task Login()
        {
            try
            {
                IsBusy = true;

                bool success = await Task.Run(() => _authService.LoginAsync(Password));

                if (success)
                {
                    Log.Information("LoginViewModel: Login successful, navigating...");
                    Completed?.Invoke();
                }
                else
                {
                    Log.Warning("LoginViewModel: Invalid password entered");
                    Password = string.Empty;
                    await _messageService.ShowErrorIncorrectPasswordAsync();
                }
            }
            catch (AppException ex)
            {
                Log.Error(ex, "LoginViewModel: Critical error during login");
                await _messageService.ShowErrorAsync(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanLogin() => !IsBusy && !string.IsNullOrWhiteSpace(Password);
    
    }
}
