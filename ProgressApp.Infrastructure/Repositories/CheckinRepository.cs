using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.Domain.Models.Journal;
using ProgressApp.Infrastructure.Data;

namespace ProgressApp.Infrastructure.Repositories
{
    public class CheckinRepository : ICheckinRepository
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CheckinRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        private async Task<DailyCheckin?> GetTodayInternalAsync(ProgressDbContext context)
        {
            var today = DateTime.Today;
            return await context.Checkins
                .Include(c => c.ActionLogs)
                .FirstOrDefaultAsync(e => e.Date.Date == today);
        }

        public async Task<DailyCheckin?> GetTodayAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            return await GetTodayInternalAsync(context);
        }
        
        public async Task SaveTodayAsync(DailyCheckin checkin)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            var entry = await GetTodayInternalAsync(context);
            
            if (entry == null)
                await context.Checkins.AddAsync(checkin);
            else
            {
                entry.Description = checkin.Description;
                entry.Result = checkin.Result;
                entry.MilestoneId = checkin.MilestoneId;
                
                foreach (var newLog in checkin.ActionLogs)
                {
                    var existingLog = entry.ActionLogs
                        .FirstOrDefault(l => l.GoalActionId == newLog.GoalActionId);

                    if (existingLog != null)
                        existingLog.IsCompleted = newLog.IsCompleted;
                    else
                        entry.ActionLogs.Add(new ActionLog
                        {
                            GoalActionId = newLog.GoalActionId,
                            IsCompleted = newLog.IsCompleted
                        });
                }
            }

            await context.SaveChangesAsync();
        }
        
        public async Task<List<DailyCheckin>> GetAllEntriesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            var entries = await context.Checkins
                        .AsNoTracking()
                        .OrderByDescending(e => e.Date)
                        .ToListAsync();

            return entries;
        }
    }
}
