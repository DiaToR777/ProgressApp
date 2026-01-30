using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace ProgressApp.Data
{
    public class ProgressDbContextFactory : IDesignTimeDbContextFactory<ProgressDbContext>
    {
        public ProgressDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProgressDbContext>();

            // Явно отримуємо шлях до робочого столу
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string folder = Path.Combine(desktop, "ProgressApp");

            // Створюємо папку, якщо її ще немає (важливо для консолі!)
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string dbPath = Path.Combine(folder, "progress.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            return new ProgressDbContext(optionsBuilder.Options);
        }
    }
}
