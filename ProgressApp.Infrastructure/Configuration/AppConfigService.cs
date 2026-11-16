using ProgressApp.Domain.Exceptions;
using Serilog;
using System.Text.Json;

namespace ProgressApp.Infrastructure.Configuration
{
    public class AppConfigService : IAppConfigService
    {
        private readonly string _path = AppPaths.ConfigPath;
        public AppConfig Load()
        {
            try
            {
                if (!File.Exists(_path))
                {
                    var defaults = new AppConfig();
                    Save(defaults);
                    Log.Debug("AppConfig: Config file not found, created with defaults");
                    return defaults;
                }                

                var json = File.ReadAllText(_path);
                var config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                Log.Debug("AppConfig: Loaded successfully");
                return config;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AppConfig: Failed to load config" );
                return new AppConfig(); //TODO throw new AppException("Msg_LoadSettingsError", isCritical: true);
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
                throw new AppException("Msg_SaveSettingsError", isCritical: true); 
            }
        }

    }
}
