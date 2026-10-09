using ProgressApp.Domain.Models.User;

namespace ProgressApp.Domain.Interfaces.IService;

public interface IUserService
{
    Task<User?> GetCurrentUserAsync();
    Task UpdateUsernameAsync(string newUsername);
}