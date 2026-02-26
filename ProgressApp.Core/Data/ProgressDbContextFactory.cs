using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            return new ProgressDbContext(optionsBuilder.Options);
        }
    }
}
