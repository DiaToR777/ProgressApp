using ProgressApp.Core.Data;
using ProgressApp.Core.Models.Journal;
using Serilog;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using Microsoft.EntityFrameworkCore;

namespace ProgressApp.Core.Services
{
    public class JournalService : IJournalService
    {
        private readonly ProgressDbContext _context;

        public JournalService(ProgressDbContext context)
        {
            _context = context;
        }

        public async Task<JournalEntry?> GetTodayAsync()
        {
            try
            {
                var today = DateTime.Today;
                return await _context.Entries
                    .FirstOrDefaultAsync(e => e.Date.Date == today)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while checking for today's entry in database.");
                throw new AppException("Msg_ErrorLoadingData");
            }
        }

        public async Task SaveTodayAsync(string description, DayResult result)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(description))
                {
                    Log.Warning("Attempted to save today's entry with empty description.");
                    throw new AppException("Msg_DescriptionEmpty");
                }

                var entry = await GetTodayAsync();
                bool isNew = entry == null;

                if (isNew)
                {
                    entry = new JournalEntry
                    {
                        Date = DateTime.Today,
                        CreatedAt = DateTime.Now
                    };
                    await _context.Entries.AddAsync(entry).ConfigureAwait(false);
                }

                entry.Description = description;
                entry.Result = result;

                if (isNew)
                    Log.Information("Creating new entry: {Description} | Result: {Result}", description, result);
                else
                    Log.Information("Updating entry: {Description} | Result: {Result}", description, result);

                await _context.SaveChangesAsync();

                Log.Information("Entry {Action} successfully. ID: {Id}", isNew ? "created" : "updated", entry.Id);
            }
            catch (Exception ex) when (ex is not AppException)
            {

                Log.Error(ex, "Error occurred while saving today's entry (Description: {Description})", description);
                throw new AppException("Msg_SaveEntryError");
            }
        }

        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            try
            {
                var entries = await _context.Entries
                            .AsNoTracking() // Обов'язково! Ми ж просто відображаємо список
                            .OrderByDescending(e => e.Date)
                            .ToListAsync()
                            .ConfigureAwait(false);

                Log.Debug("Fetched {Count} entries from database.", entries.Count);
                return entries;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch all journal entries.");
                throw new AppException("Msg_ErrorLoadingData");
            }
        }

        //public int GetStreak(List<JournalEntry> entries)
        //{
        //    int streak = 0;

        //    foreach (var entry in entries)
        //    {
        //        if (entry.Result == DayResult.Success)
        //            streak++;
        //        else
        //            streak = 0; 
        //    }

        //    return streak;
        //}
    }
}

