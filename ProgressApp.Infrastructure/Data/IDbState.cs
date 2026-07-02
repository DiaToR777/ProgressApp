namespace ProgressApp.Infrastructure.Data;

public interface IDbState
{
    string DbPath { get; }
    bool IsAuthenticated { get; }
    void SetPassword(string password);
    string GetConnectionString(string? passwordOverride = null);
}
