using ProgressApp.Domain.Models.Journal;

namespace ProgressApp.Domain.Interfaces.IRepository
{
    public interface ICheckinRepository
    {
        Task<DailyCheckin?> GetTodayAsync();
        Task SaveTodayAsync(DailyCheckin checkin);
        Task<List<DailyCheckin>> GetAllEntriesAsync();

    }
}
