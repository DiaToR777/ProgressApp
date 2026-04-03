using Microsoft.EntityFrameworkCore;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Journal;
using ProgressApp.Core.Models.Settings;

namespace ProgressApp.Core.Data
{
    public class ProgressDbContext : DbContext
    {
        private readonly IDbState _dbState;
        public ProgressDbContext(DbContextOptions<ProgressDbContext> options, IDbState dbState)
        : base(options)
        {
            _dbState = dbState;
        }
        public DbSet<JournalEntry> Entries { get; set; } = null!;
        public DbSet<AppSettings> Settings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JournalEntry>(entity =>
            {
                entity.HasIndex(e => e.Date).IsUnique().HasDatabaseName("IX_Entries_Date");
                entity.Property(e => e.Description).HasMaxLength(1500);
            });

            modelBuilder.Entity<AppSettings>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.Key).HasMaxLength(35);
                entity.Property(e => e.Value).HasMaxLength(150);
            });

            base.OnModelCreating(modelBuilder);
        }
        public async Task InitializeAsync()
        {
            await Database.MigrateAsync();

            if (!await Settings.AnyAsync(s => s.Key == SettingsKeys.Goal))
            {
                Settings.Add(new AppSettings { Key = SettingsKeys.Goal, Value = "" });
                await SaveChangesAsync();
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _dbState.GetConnectionString();
                optionsBuilder.UseSqlite(connectionString);
            }
        }
    }
}
