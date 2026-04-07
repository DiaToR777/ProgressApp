using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace ProgressApp.Core.Data
{
    public class ProgressDbContextFactory : IDesignTimeDbContextFactory<ProgressDbContext>
    {
        //Design-Time factory
        public ProgressDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProgressDbContext>();

            string dbPath = DbConstants.GetDefaultDbPath();

            var designTimeDbState = new DbState(dbPath);
            designTimeDbState.SetPassword("12345");

            var connectionString = designTimeDbState.GetConnectionString();

            optionsBuilder.UseSqlite(connectionString);

            optionsBuilder.LogTo(Log.Information, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);


            return new ProgressDbContext(optionsBuilder.Options, designTimeDbState);
        }
    }
}
