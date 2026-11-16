using Microsoft.Data.Sqlite;

namespace ProgressApp.Infrastructure.Data
{
    public class DbState : IDbState
    {
        public string DbPath { get; }
        private string? _password;

        public bool IsAuthenticated => !string.IsNullOrEmpty(_password);

        public DbState(string dbPath) => DbPath = dbPath;

        public void SetPassword(string password) => _password = password;

        public string GetConnectionString(string? passwordOverride = null)
        {
            return new SqliteConnectionStringBuilder
            {
                DataSource = DbPath,
                Password = passwordOverride ?? _password
            }.ToString();
        }
    }
}