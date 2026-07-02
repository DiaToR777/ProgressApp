using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Enums;
using Serilog;

namespace ProgressApp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<DbStatus> GetDbStatusAsync()
        {
            if (!_authRepository.IsDatabaseCreated()) return DbStatus.NotCreated;
            if (await _authRepository.IsDatabaseEncryptedAsync()) return DbStatus.Encrypted;
            return DbStatus.Unencrypted;
        }

        public async Task<bool> LoginAsync(string password)
        {
            Log.Information("AuthService: Login attempt started");
            try
            {
                if (await _authRepository.CanConnectAsync(password))
                {
                    Log.Information("AuthService: Login successful");
                    return true;
                }
                Log.Warning("AuthService: Login failed. Wrong password");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AuthService: Critical error during login");
                throw new AppException("Msg_ErrorLoginFailed", isCritical: true);
            }
        }

        public async Task<bool> RegisterAsync(string password)
        {
            Log.Information("AuthService: Starting database registration and encryption...");
            try
            {
                await _authRepository.InitializeDatabaseAsync(password);
                Log.Information("AuthService: Database initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "AuthService: FAILED to register database");
                throw new AppException("Msg_ErrorSetupFailed", isCritical: true);
            }
        }

        public async Task ChangePasswordAsync(string newPassword)
        {
            Log.Information("AuthService: Starting password change process...");
            try
            {
                await _authRepository.EditPasswordAsync(newPassword);
                Log.Information("AuthService: Password changed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AuthService: Error during password change");
                throw new AppException("Msg_ChangePasswordFailedError", isCritical: true);
            }
        }

        public async Task SetPasswordAsync(string password)
        {
            Log.Information("AuthService: Setting password...");
            try
            {
                await _authRepository.ExportToNewDatabaseAsync(password);
                Log.Information("AuthService: Password set successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AuthService: Error during password set");
                throw new AppException("Msg_ChangePasswordFailedError", isCritical: true);
            }
        }

        public async Task RemovePasswordAsync()
        {
            Log.Information("AuthService: Removing password...");
            try
            {
                await _authRepository.ExportToNewDatabaseAsync(string.Empty);
                Log.Information("AuthService: Password removed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AuthService: Error during password removal");
                throw new AppException("Msg_ChangePasswordFailedError", isCritical: true);
            }
        }
    }
}
