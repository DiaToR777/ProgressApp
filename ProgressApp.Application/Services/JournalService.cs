using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Journal;
using Serilog;

namespace ProgressApp.Application.Services
{
    public class JournalService : IJournalService
    {
        private readonly IJournalRepository _journalRepository;

        public JournalService(IJournalRepository journalRepository)
        {
            _journalRepository = journalRepository;
        }
        public async Task<JournalEntry?> GetTodayAsync()
        {
            try
            {
                return await _journalRepository.GetTodayAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while checking for today's entry in database.");
                throw new AppException("Msg_ErrorLoadingData", isCritical: true);
            }
        }

        public async Task SaveTodayAsync(string description, DayResult result)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(description))
                {
                    Log.Warning("Attempted to save today's entry with empty description.");
                    throw new AppException("Msg_DescriptionEmpty");
                }

                await _journalRepository.SaveTodayAsync(description, result);  

                Log.Information("Entry saved successfully.");
            }
            catch (Exception ex) when (ex is not AppException)
            {
                Log.Error(ex, "Error occurred while saving today's entry");
                throw new AppException("Msg_SaveEntryError", isCritical: true);
            }
        }

        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            try
            {
                var entries = await _journalRepository.GetAllEntriesAsync();
                Log.Debug("Fetched {Count} entries from database.", entries.Count);
                return entries;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to fetch all journal entries.");
                throw new AppException("Msg_ErrorLoadingData", isCritical: true);
            }
        }                

    }
}
