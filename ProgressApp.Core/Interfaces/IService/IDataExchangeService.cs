
namespace ProgressApp.Core.Interfaces.IService
{
    public interface IDataExchangeService
    {
        Task ExportToCsvAsync(string filePath);
        Task ImportFromCsvAsync(string filePath);
    }
}
