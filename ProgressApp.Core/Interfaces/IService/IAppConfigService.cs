using ProgressApp.Core.Models.Config;
namespace ProgressApp.Core.Interfaces.IService
{
    public interface IAppConfigService 
    {
        AppConfig Load();
        void Save(AppConfig config);
    }
}
