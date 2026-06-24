using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Infrastructure.Data;

namespace ProgressApp.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDbState _dbState;

        public AuthRepository(IServiceScopeFactory scopeFactory, IDbState dbState)
        {
            _scopeFactory = scopeFactory;
            _dbState = dbState;
        }
        public bool IsDatabaseCreated() => File.Exists(_dbState.DbPath);
        public async Task<bool> IsDatabaseEncryptedAsync()
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
        public async Task EditPasswordAsync(string password)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA rekey = '{password.Replace("'", "''")}';";
            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
            SqliteConnection.ClearAllPools();
            _dbState.SetPassword(password);
        }
        public async Task ExportToNewDatabaseAsync(string newPassword)
        {
            var tempPath = _dbState.DbPath + ".tmp";
            try
            {
                using var connection = new SqliteConnection(_dbState.GetConnectionString());
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                var safePassword = newPassword.Replace("'", "''");
                command.CommandText = $"ATTACH DATABASE '{tempPath}' AS target KEY '{safePassword}';";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "SELECT sqlcipher_export('target');";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DETACH DATABASE target;";
                await command.ExecuteNonQueryAsync();
                connection.Close();
                SqliteConnection.ClearAllPools();
                File.Delete(_dbState.DbPath);
                File.Move(tempPath, _dbState.DbPath);
                _dbState.SetPassword(newPassword);
            }
            catch
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
                throw;
            }
        }

        public async Task InitializeDatabaseAsync(string password)
        {
            _dbState.SetPassword(password);
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
            await context.InitializeAsync();
            await context.SaveChangesAsync();
        }

        public async Task<bool> CanConnectAsync(string password)
        {
            _dbState.SetPassword(password);
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
            return await context.Database.CanConnectAsync();
        }
    }
}
