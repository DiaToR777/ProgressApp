using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace ProgressApp.Core.Data
{
    public class ProgressDbContextFactory : IDesignTimeDbContextFactory<ProgressDbContext>
    {
        public ProgressDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProgressDbContext>();

            string localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string folder = Path.Combine(localappdata, "ProgressApp");

            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string dbPath = Path.Combine(folder, "progress.db");

            var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Password = "12345" // ВАЖНО: тот же, что в приложении
            }.ToString();

            optionsBuilder.UseSqlite(connectionString);

            optionsBuilder.LogTo(Log.Information, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
        
            return new ProgressDbContext(optionsBuilder.Options);
        }
    }
}
