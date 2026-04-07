
namespace ProgressApp.Core.Data
{
    public static class DbConstants
    {
        public static string GetDefaultDbPath()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string folder = Path.Combine(localAppData, "ProgressApp");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return Path.Combine(folder, "progress.db");
        }
    }
}
