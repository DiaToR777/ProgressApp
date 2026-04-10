using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Core.Data;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using Serilog;

namespace ProgressApp.Core.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IDbState _dbState;
        private readonly IServiceScopeFactory _scopeFactory;
        public AuthService(IDbState dbState, IServiceScopeFactory scopeFactory)
        {
            _dbState = dbState;
            _scopeFactory = scopeFactory;
        }
        public bool IsDatabaseCreated()
        {
            Log.Debug("AuthService: Checking if database file exists at {Path}", _dbState.DbPath);
            try
            {
                bool isExists = File.Exists(_dbState.DbPath);
                if (!isExists) Log.Warning("AuthService: Database file NOT found at {Path}", _dbState.DbPath);
                return isExists;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AuthService: Error while checking database file existence");
                throw new AppException("Msg_ErrorDbAccessFailed");
            }
        }
        public async Task<bool> LoginAsync(string password)
        {
            Log.Information("AuthService: Login attempt started");
            try
            {
                _dbState.SetPassword(password);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
                    if (await context.Database.CanConnectAsync())
                    {
                        Log.Information("AuthService: Login successful. Database connection established");
                        return true;
                    }
                    Log.Warning("AuthService: Login failed. Wrong password");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AuthService: Critical error during login process");
                throw new AppException("Msg_ErrorLoginFailed");
            }
        }

        public async Task<bool> RegisterAsync(string password)
        {
            Log.Information("AuthService: Starting database registration and encryption...");
            try
            {
                _dbState.SetPassword(password);
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();


                    Log.Debug("AuthService: Initializing database schema...");
                    await context.InitializeAsync();

                    await context.SaveChangesAsync();
                    Log.Information("AuthService: Database registered and initialized successfully at {Path}", _dbState.DbPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "AuthService: FAILED to register new database. Path: {Path}", _dbState.DbPath);
                throw new AppException("Msg_ErrorSetupFailed");
            }
        }

        public async Task ChangePasswordAsync(string newPassword)
        {
            Log.Information("AuthService: Starting password change process...");
            await EditPassword(newPassword);
        }

        public async Task RemovePasswordAsync()
        {
            Log.Information("AuthService: Starting password removal process..."); 
            await EditPassword(string.Empty);
        }

        private async Task EditPassword(string password)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
                    var connection = context.Database.GetDbConnection();

                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        var safePassword = password?.Replace("'", "''") ?? "";
                        command.CommandText = $"PRAGMA rekey = '{safePassword}';";
                        await command.ExecuteNonQueryAsync();
                    }
                }

                if(password != null)
                _dbState.SetPassword(password);

                Log.Information("AuthService: Password changed successfully.");
                return;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AuthService: Error during password change");
                throw new AppException("Msg_ChangePasswordFailedError"); 
            }

        }

        public async Task<bool> IsDatabaseEncrypted()
        {
            using var connection = new SqliteConnection(_dbState.GetConnectionString(string.Empty));
            try
            {
                
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT count(*) FROM sqlite_master;";
                await command.ExecuteScalarAsync();

                return false;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 26 || ex.SqliteExtendedErrorCode == 3390)
            {
                return true;
            }
        }

    }
}
