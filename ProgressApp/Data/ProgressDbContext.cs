using Microsoft.EntityFrameworkCore;
using ProgressApp.Model.Journal;
using ProgressApp.Model.Settings;

namespace ProgressApp.Data
{
    public class ProgressDbContext : DbContext
    {

        public ProgressDbContext(DbContextOptions<ProgressDbContext> options) : base(options) { }
        public DbSet<JournalEntry> Entries { get; set; } = null!;
        public DbSet<AppSettings> Settings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JournalEntry>(entity =>
            {
                entity.HasIndex(e => e.Date).IsUnique().HasDatabaseName("IX_Entries_Date");
                entity.Property(e => e.Description).HasMaxLength(1000);
            });

            modelBuilder.Entity<AppSettings>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.Key).HasMaxLength(100);
                entity.Property(e => e.Value).HasMaxLength(500);
            });

            base.OnModelCreating(modelBuilder);
        }        // Метод для ініціалізації БД
        public void Initialize()
        {
            // Використовуй Migrate(), щоб працювали міграції, а не просто створення файлу
            Database.Migrate();

            if (!Settings.Any())
            {
                Settings.AddRange(
                    new AppSettings { Key = "Username", Value = "" },
                    new AppSettings { Key = "Goal", Value = "" },
                    new AppSettings { Key = "Theme", Value = "Light" }
                );
                SaveChanges();
            }
        }
    }
}
