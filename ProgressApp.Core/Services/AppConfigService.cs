using ProgressApp.Core.Configuration;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Config;
using Serilog;
using System.Text.Json;

namespace ProgressApp.Core.Services
{
    public class AppConfigService : IAppConfigService
    {
        private readonly string _path = AppPaths.ConfigPath;
        public AppConfig Load()
        {
            try
            {
                if (!File.Exists(_path))
                    return new AppConfig();

                var json = File.ReadAllText(_path);
                var config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                Log.Debug("AppConfig: Saved successfully");
                return config;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AppConfig: Failed to load config");
                return new AppConfig();
            }
        }

        public void Save(AppConfig config)
        {
            try
            {
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_path, json);
                Log.Debug("AppConfig: Saved successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AppConfig: Failed to save config");
                throw new AppException("Msg_SaveSettingsError"); 
            }
        }

    }
}
