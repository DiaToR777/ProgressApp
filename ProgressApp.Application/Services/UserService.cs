using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.User;
using Serilog;

namespace ProgressApp.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        try
        {
            return await _userRepository.GetUserAsync();
        }
        catch (Exception ex) when (ex is not AppException)
        {
            Log.Error(ex, "UserService: Error while getting current user");
            throw new AppException("Msg_ErrorLoadingUser", isCritical: true); 
        }
    }

    public async Task UpdateUsernameAsync(string newUsername)
    {
        if (string.IsNullOrWhiteSpace(newUsername))
            throw new AppException("Msg_UsernameEmpty");

        if (newUsername.Length > 50)
            throw new AppException("Msg_UsernameTooLong");

        var trimmedUsername = newUsername.Trim();

        try
        {
            var user = await _userRepository.GetUserAsync();
            if (user == null)
                throw new AppException("Msg_UserNotFound");

            user.Username = trimmedUsername;

            await _userRepository.UpdateUserAsync(user);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            Log.Error(ex, "UserService: Failed to update username.");
            throw new AppException("Msg_ErrorUpdatingUser", isCritical: true);
        }
    }
}