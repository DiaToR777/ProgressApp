using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using ProgressApp.Infrastructure.Configuration;

namespace ProgressApp.Infrastructure.Data
{
    public class ProgressDbContextFactory : IDesignTimeDbContextFactory<ProgressDbContext>
    {
        //Design-Time factory
        public ProgressDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProgressDbContext>();

            string dbPath = AppPaths.GetDefaultDbPath();

            var designTimeDbState = new DbState(dbPath);
            designTimeDbState.SetPassword("12345");

            var connectionString = designTimeDbState.GetConnectionString();

            optionsBuilder.UseSqlite(connectionString);

            optionsBuilder.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);


            return new ProgressDbContext(optionsBuilder.Options, designTimeDbState);
        }
    }
}
