using ProgressApp.Core.Configuration;
using Serilog;
using Serilog.Exceptions;
using System.IO;

namespace ProgressApp.WpfUI.LogConfig
{
    public static class LoggerConfigurator
    {
        public static void Setup()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Debug()
                .WriteTo.File(
                    path: Path.Combine(AppPaths.LogFolder, "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .CreateLogger();
        }
    }
}
