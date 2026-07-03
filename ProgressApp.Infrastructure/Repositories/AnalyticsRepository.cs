using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Models.Journal;
using ProgressApp.Infrastructure.Data;

namespace ProgressApp.Infrastructure.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public AnalyticsRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        //todo actions streak
        public async Task<List<DailyCheckin>> GetEntriesByDateRangeAsync(DateTime from, DateTime to)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            return await context.Checkins
                .AsNoTracking()
                .Where(e => e.Date >= from && e.Date <= to)
                .OrderBy(e => e.Date)
                .ToListAsync();
        }
        public async Task<List<DailyCheckin>> GetEntriesForStreakAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            return await context.Checkins
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<DateTime?> GetFirstEntryDateAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            var firstEntry = await context.Checkins
                .AsNoTracking()
                .OrderBy(e => e.Date)
                .FirstOrDefaultAsync();
            return firstEntry?.Date;
        }
    }
}
