using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Models.Journal;
using ProgressApp.Infrastructure.Data;

namespace ProgressApp.Infrastructure.Repositories
{
    public class JournalRepository : IJournalRepository
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public JournalRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        private async Task<JournalEntry?> GetTodayInternalAsync(ProgressDbContext context)
        {
            var today = DateTime.Today;
            return await context.Entries
                .FirstOrDefaultAsync(e => e.Date.Date == today);
        }

        public async Task<JournalEntry?> GetTodayAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            return await GetTodayInternalAsync(context);

        }
        public async Task SaveTodayAsync(string description, DayResult result)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            var entry = await GetTodayInternalAsync(context);
            bool isNew = entry == null;

            if (isNew)
            {
                entry = new JournalEntry
                {
                    Date = DateTime.Today,
                    CreatedAt = DateTime.Now
                };
                await context.Entries.AddAsync(entry);
            }

            entry.Description = description;
            entry.Result = result;
            
            await context.SaveChangesAsync();
        }
        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            var entries = await context.Entries
                        .AsNoTracking()
                        .OrderByDescending(e => e.Date)
                        .ToListAsync();

            return entries;
        }
    }
}
