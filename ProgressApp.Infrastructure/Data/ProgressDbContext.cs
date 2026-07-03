using Microsoft.EntityFrameworkCore;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.Domain.Models.Journal;
using ProgressApp.Domain.Models.Settings;
using ProgressApp.Domain.Models.User;

namespace ProgressApp.Infrastructure.Data
{
    public class ProgressDbContext : DbContext
    {
        private readonly IDbState _dbState;
        public ProgressDbContext(DbContextOptions<ProgressDbContext> options, IDbState dbState)
        : base(options)
        {
            _dbState = dbState;
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Goal> Goals { get; set; } = null!;
        public DbSet<Milestone> Milestones { get; set; } = null!;
        public DbSet<GoalAction> GoalActions { get; set; } = null!;
        public DbSet<ActionLog> ActionLogs { get; set; } = null!;
        public DbSet<DailyCheckin> Checkins { get; set; } = null!;
        public DbSet<AppSettings> Settings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Username).HasMaxLength(50);
                entity.HasMany(u => u.Goals)
                    .WithOne(g => g.User)
                    .HasForeignKey(g => g.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<Goal>(entity =>
            {
                entity.Property(g => g.Title).HasMaxLength(150);
                entity.Property(g => g.Description).HasMaxLength(1500);

                entity.HasMany(g => g.Milestones)
                    .WithOne(m => m.Goal)
                    .HasForeignKey(m => m.GoalId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Milestone>(entity =>
            {
                entity.Ignore(m => m.EndDate); 

                entity.HasMany(m => m.Actions)
                    .WithOne(a => a.Milestone)
                    .HasForeignKey(a => a.MilestoneId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(m => m.Checkins)
                    .WithOne(c => c.Milestone)
                    .HasForeignKey(c => c.MilestoneId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            
            modelBuilder.Entity<GoalAction>(entity =>
            {
                entity.Property(a => a.Title).HasMaxLength(150);

                entity.HasMany(a => a.Logs)
                    .WithOne(l => l.GoalAction)
                    .HasForeignKey(l => l.GoalActionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DailyCheckin>(entity =>
            {
                entity.Property(c => c.Description).HasMaxLength(1500);

                entity.HasIndex(c => new { c.MilestoneId, c.Date })
                    .IsUnique()
                    .HasDatabaseName("IX_Checkins_Milestone_Date");

                entity.HasMany(c => c.ActionLogs)
                    .WithOne(l => l.DailyCheckin)
                    .HasForeignKey(l => l.DailyCheckinId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<ActionLog>(entity =>
            {
                entity.HasIndex(l => new { l.DailyCheckinId, l.GoalActionId })
                    .IsUnique()
                    .HasDatabaseName("IX_ActionLogs_Checkin_Action");
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
