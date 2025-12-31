using Microsoft.EntityFrameworkCore;
using ProgressApp.Model.Journal;
using ProgressApp.Model.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ProgressApp.Data
{
    public class ProgressDbContext : DbContext
    {
        private readonly string _dbPath;

        public ProgressDbContext(string dbPath)
        {
            _dbPath = dbPath;
        }
        public DbSet<JournalEntry> Entries { get; set; } = null!;
        public DbSet<AppSettings> Settings { get; set; } = null!;


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Підключення до SQLite
            optionsBuilder.UseSqlite($"Data Source={_dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ===== Налаштування для JournalEntryDTO =====

            modelBuilder.Entity<JournalEntry>(entity =>
            {
                // Унікальний індекс на дату (один запис = один день)
                entity.HasIndex(e => e.Date)
                      .IsUnique()
                      .HasDatabaseName("IX_Entries_Date");

                // Максимальна довжина для Description
                entity.Property(e => e.Description)
                      .HasMaxLength(1000);
            });

            // ===== Налаштування для AppSettings =====

            modelBuilder.Entity<AppSettings>(entity =>
            {
                // Key як Primary Key (вже є через [Key] атрибут)
                entity.HasKey(e => e.Key);

                // Максимальна довжина для ключа
                entity.Property(e => e.Key)
                      .HasMaxLength(100);

                // Максимальна довжина для значення
                entity.Property(e => e.Value)
                      .HasMaxLength(500);
            });

            base.OnModelCreating(modelBuilder);
        }

        // Метод для ініціалізації БД
        public void Initialize()
        {

            if (!Settings.Any())
            {
                Settings.AddRange(
                    new AppSettings { Key = "Username"},
                    new AppSettings { Key = "Goal"}
                );
                SaveChanges();
            }
            // Створює БД якщо не існує
            Database.EnsureCreated();

            // Додаємо початкові налаштування якщо їх немає
        }
    }
}
