using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using Serilog;
using System.Windows.Input;

namespace ProgressApp.WpfUI.ViewModels.Login
{
    public class LoginViewModel : ViewModelBase
    {
        public Action? Completed; //TOOO Event

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
                   if (IsBusy) return;
                   try
                   {

                       IsBusy = true;
                       bool success = await Task.Run(async () =>
                       {
                           return await _authService.LoginAsync(Password);
                       });
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

                       Log.Debug("LoginViewModel: UI Success notification shown to user");
                   }
                   catch (AppException ex)
                   {
                       Log.Error(ex, "LoginViewModel: Critical error loging");
                       await messageService.ShowErrorAsync(ex);
                   }
                   finally
                   {
                       IsBusy = false;
                   }
               },
               canExecute: _ => !string.IsNullOrWhiteSpace(Password) && !IsBusy
               );
        }
    }
}
