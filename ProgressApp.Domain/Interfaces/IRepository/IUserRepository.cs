using ProgressApp.Domain.Models.User;

namespace ProgressApp.Domain.Interfaces.IRepository;

public interface IUserRepository
{
    Task<User?> GetUserAsync(); 
    Task<User> CreateUserAsync(User user);
}