
namespace ProgressApp.Core.Interfaces.IService
{
    public interface IAuthService
    {
        bool IsDatabaseCreated();
        Task<bool> LoginAsync(string password);
        Task<bool> RegisterAsync(string password);
        Task ChangePasswordAsync(string newPassword);
        
        Task RemovePasswordAsync();
        Task<bool> IsDatabaseEncrypted();


    }
}
