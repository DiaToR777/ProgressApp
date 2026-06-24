using ProgressApp.Domain.Models.Enums;

namespace ProgressApp.Domain.Interfaces.IService;

public interface IAuthService
{
    Task<DbStatus> GetDbStatusAsync();
    Task<bool> LoginAsync(string password);
    Task<bool> RegisterAsync(string password);
    Task ChangePasswordAsync(string newPassword);
    Task RemovePasswordAsync();
    Task SetPasswordAsync(string password);


}
