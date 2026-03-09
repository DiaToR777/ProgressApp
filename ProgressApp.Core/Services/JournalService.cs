using ProgressApp.Core.Data;
using ProgressApp.Core.Models.Journal;
using Serilog;

namespace ProgressApp.Core.Services
{
    public class JournalService : IJournalService
    {
        private readonly ProgressDbContext _context;

        public JournalService(ProgressDbContext context)
        {
            _context = context;
        }

        public JournalEntry? GetToday()
        {
            try
            {
                var today = DateTime.Today;
                return _context.Entries.FirstOrDefault(e => e.Date.Date == today);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while checking for today's entry in database.");
                return null;
            }
        }

        public void SaveToday(string description, DayResult result)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(description))
                {
                    Log.Warning("Attempted to save today's entry with empty description.");
                    throw new ArgumentException("Опис дня не може бути порожнім!");
                }

                var entry = GetToday();
                bool isNew = entry == null;

                if (isNew)
                {
                    entry = new JournalEntry
                    {
                        Date = DateTime.Today,
                        CreatedAt = DateTime.Now
                    };
                    _context.Entries.Add(entry);
                }

                entry.Description = description;
                entry.Result = result;

                if (isNew)
                    Log.Information("Creating new entry: {Description} | Result: {Result}", description, result);
                else
                    Log.Information("Updating entry: {Description} | Result: {Result}", description, result);

                _context.SaveChanges();

                Log.Information("Entry {Action} successfully. ID: {Id}", isNew ? "created" : "updated", entry.Id);
            }
            catch (Exception ex)
            {

                Log.Error(ex, "Error occurred while saving today's entry (Description: {Description})", description);
                throw;
            }
        }

        public List<JournalEntry> GetAllEntries()
        {
            try
            {
                var entries = _context.Entries
                               .OrderByDescending(e => e.Date)
                               .ToList();

                Log.Debug("Fetched {Count} entries from database.", entries.Count);
                return entries;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch all journal entries.");
                return new List<JournalEntry>(); 
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

