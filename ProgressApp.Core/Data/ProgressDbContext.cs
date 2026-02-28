using Microsoft.EntityFrameworkCore;
using ProgressApp.Core.Models.Journal;
using ProgressApp.Core.Models.Settings;

namespace ProgressApp.Core.Data
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
                entity.Property(e => e.Key).HasMaxLength(35);
                entity.Property(e => e.Value).HasMaxLength(150);
            });

            base.OnModelCreating(modelBuilder);
        }
        public void Initialize()
        {
            Database.Migrate();

            var defaultSettings = new List<AppSettings>
                    {
                        new AppSettings { Key = "Username", Value = "" },
                        new AppSettings { Key = "Goal", Value = "" },
                        new AppSettings { Key = "Theme", Value = "Light" },
                        new AppSettings { Key = "Language", Value = "en-US" }
                    };

            bool changed = false;
            foreach (var setting in defaultSettings)
            {
                if (!Settings.Any(s => s.Key == setting.Key))
                {
                    Settings.Add(setting);
                    changed = true;
                }
            }
            if (changed)
                SaveChanges();
        }
    }
}
