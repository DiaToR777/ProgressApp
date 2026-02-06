using ProgressApp.Data;
using ProgressApp.Model.Settings;

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

        public AppTheme GetTheme()
        {
            var themeValue = _context.Settings.FirstOrDefault(s => s.Key == SettingsKeys.Theme)?.Value;

            //if (string.IsNullOrEmpty(themeValue))
            //    return AppTheme.Light;

            return Enum.TryParse(themeValue, out AppTheme result) ? result : AppTheme.Light;
        }
            
        public void SaveSettings(string username, string goal, AppTheme theme)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Ім'я не може бути порожнім!");

            if (string.IsNullOrWhiteSpace(goal))
                throw new ArgumentException("Ціль має бути заповнена!");

            var u = _context.Settings.FirstOrDefault(s => s.Key == SettingsKeys.Username);
            if (u != null) u.Value = username;

            var g = _context.Settings.FirstOrDefault(s => s.Key == SettingsKeys.Goal);
            if (g != null) g.Value = goal;

            var t = _context.Settings.FirstOrDefault(s => s.Key == SettingsKeys.Theme);
            if (t != null) t.Value = theme.ToString();

            _context.SaveChanges();
        }

    }

}
