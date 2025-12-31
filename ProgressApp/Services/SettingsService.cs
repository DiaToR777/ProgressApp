using Microsoft.EntityFrameworkCore;
using ProgressApp.Data;
using ProgressApp.Model.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ProgressApp.Services
{
    public class SettingsService
    {
        private ProgressDbContext _context;
        public SettingsService()
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var folder = Path.Combine(desktop, "ProgressApp");
            Directory.CreateDirectory(folder);

            _context = new ProgressDbContext(Path.Combine(folder, "progress.db"));
            _context.Initialize();
        }

        public bool IsFirstRun()
        {
            var usernameSetting = _context.Settings.FirstOrDefault(s => s.Key == SettingsKeys.Username);
            return usernameSetting == null || string.IsNullOrWhiteSpace(usernameSetting.Value);
        }

        public void SaveInitial(string username, string goal)
        {
            var user = _context.Settings.First(s => s.Key == SettingsKeys.Username);
            user.Value = username;

            var g = _context.Settings.First(s => s.Key == SettingsKeys.Goal);
            g.Value = goal;

            _context.SaveChanges();
        }
    }
}
