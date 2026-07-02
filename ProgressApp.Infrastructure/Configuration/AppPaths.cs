namespace ProgressApp.Infrastructure.Configuration
{
    public static class AppPaths
    {
        private static readonly string BaseFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ProgressApp");

        public static string LogFolder => Path.Combine(BaseFolder, "logs");
        public static string DbPath => Path.Combine(BaseFolder, "progress.db");
        public static string ConfigPath => Path.Combine(BaseFolder, "appsettings.json");

        public static void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(BaseFolder)) Directory.CreateDirectory(BaseFolder);
            if (!Directory.Exists(LogFolder)) Directory.CreateDirectory(LogFolder);
        }

        public static string GetDefaultDbPath()
        {
            string folder = Path.Combine(BaseFolder, "ProgressApp");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return Path.Combine(folder, "progress.db");
        }

    }
}
