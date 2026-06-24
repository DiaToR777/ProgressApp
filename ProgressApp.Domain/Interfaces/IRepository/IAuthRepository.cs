namespace ProgressApp.Domain.Interfaces.IRepository;

public interface IAuthRepository
{
    bool IsDatabaseCreated();
    Task<bool> IsDatabaseEncryptedAsync();
    Task ExportToNewDatabaseAsync(string newPassword);
    Task EditPasswordAsync(string password);
    Task InitializeDatabaseAsync(string password); 
    Task<bool> CanConnectAsync(string password);
}
