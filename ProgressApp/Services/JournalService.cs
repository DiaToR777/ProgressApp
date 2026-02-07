using ProgressApp.Data;
using ProgressApp.Model.Journal;
using ProgressApp.Model.Settings;

namespace ProgressApp.Services
{
    public class JournalService
    {
        private readonly ProgressDbContext _context;

        public JournalService(ProgressDbContext context)
        {
            _context = context;
        }

        public JournalEntry? GetToday()
        {
            var today = DateTime.Today;
            return _context.Entries.FirstOrDefault(e => e.Date.Date == today);
        }

        public void SaveToday(string description, DayResult result)
        {
            var entry = GetToday();

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Опис дня не може бути порожнім!");

            if (entry == null)
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

            _context.SaveChanges();
        }

        public List<JournalEntry> GetAllEntries()
        {
            return _context.Entries
                           .OrderByDescending(e => e.Date)
                           .ToList();
        }

        public int GetStreak(List<JournalEntry> entries)
        {
            int streak = 0;

            foreach (var entry in entries)
            {
                if (entry.Result == DayResult.Success)
                    streak++;
                else
                    streak = 0; 
            }

            return streak;
        }
    }
}

