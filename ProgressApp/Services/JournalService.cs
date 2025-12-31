using ProgressApp.Data;
using ProgressApp.Model.Journal;
using ProgressApp.Model.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressApp.Services
{
    public class JournalService
    {
        private readonly ProgressDbContext _context;

        public JournalService()
        {

            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var folder = Path.Combine(desktop, "ProgressApp");
            Directory.CreateDirectory(folder);

            _context = new ProgressDbContext(Path.Combine(folder, "progress.db"));
            //_context.Initialize();

        }

        public JournalEntry? GetToday()
        {
            var today = DateTime.Today;
            return _context.Entries.FirstOrDefault(e => e.Date.Date == today);
        }

        public void SaveToday(string description, DayResult result)
        {
            var entry = GetToday();

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
                    streak = 0; // сброс стрика при Relapse
            }

            return streak;
        }
    }
}

