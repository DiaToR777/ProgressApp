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
        public SettingsService(ProgressDbContext context)
        {
            _context = context;
        }

        public bool IsFirstRun()
        {
            var usernameSetting = _context.Settings.FirstOrDefault(s => s.Key == SettingsKeys.Username);
            return usernameSetting == null || string.IsNullOrWhiteSpace(usernameSetting.Value);
        }

        public string GetUserName()
    => _context.Settings.FirstOrDefault(s => s.Key == SettingsKeys.Username)?.Value ?? "";
        public string GetGoal()
    => _context.Settings.FirstOrDefault(s => s.Key == SettingsKeys.Goal)?.Value ?? "";

        public void SaveSettings(string username, string goal)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Ім'я не може бути порожнім!");

            if (string.IsNullOrWhiteSpace(goal))
                throw new ArgumentException("Ціль має бути заповнена!");

            var u = _context.Settings.FirstOrDefault(s => s.Key == SettingsKeys.Username);
            if (u != null) u.Value = username;

            var g = _context.Settings.FirstOrDefault(s => s.Key == SettingsKeys.Goal);
            if (g != null) g.Value = goal;

            _context.SaveChanges();
        }
    }
}
