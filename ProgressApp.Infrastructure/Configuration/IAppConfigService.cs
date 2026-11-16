namespace ProgressApp.Infrastructure.Configuration;

public interface IAppConfigService 
{
    AppConfig Load();
    void Save(AppConfig config);
}
