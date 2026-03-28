using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using Serilog;
using System.Windows.Input;

namespace ProgressApp.WpfUI.ViewModels.Login
{
    public class LoginViewModel : ViewModelBase
    {
        public Action? Completed;

        private readonly IAuthService _authService;
        private readonly IMessageService _messageService;
        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        public ICommand LoginCommand { get; }
        public LoginViewModel(IAuthService authService, IMessageService messageService)
        {
            _authService = authService;
            _messageService = messageService;

            LoginCommand = new RelayCommand(
               executeAsync: async _ =>
               {
                   try
                   {
                       await LoginAsync();
                       Log.Debug("LoginViewModel: UI Success notification shown to user");

                   }
                   catch (AppException ex)
                   {
                       Log.Error(ex, "LoginViewModel: Critical error loging");
                       _messageService.ShowError(ex);
                   }
               },
               canExecute: _ => !string.IsNullOrWhiteSpace(Password)
               );
        }

        private async Task LoginAsync()
        {
            var isLogined = await _authService.LoginAsync(Password);
            if (isLogined)
            {
                Completed?.Invoke();
            }
            else
            {
                _password = string.Empty;
                _messageService.ShowErrorIncorrectPassword();
                Log.Debug("LoginViewModel: UI Success notification shown to user");
            }
        }
    }
}
