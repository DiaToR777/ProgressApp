using CsvHelper;
using CsvHelper.Configuration;
using ProgressApp.Domain.Exceptions;
using ProgressApp.Domain.Interfaces.IRepository;
using ProgressApp.Domain.Interfaces.IService;
using ProgressApp.Domain.Models.Journal;
using Serilog;
using System.Globalization;


namespace ProgressApp.Application.Services
{
    public class DataExchangeService : IDataExchangeService
    {
        //TODO new goals and checkins integration
        private readonly CsvConfiguration _csvConfig;
        private readonly IDataExchangeRepository _dataExchangeRepository;

        public DataExchangeService(IDataExchangeRepository dataExchangeRepository)
        {
            _dataExchangeRepository = dataExchangeRepository;
            _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower(),
                DetectDelimiter = true
            };
        }

        public async Task ExportToCsvAsync(string filePath)
        {
            try
            {
                var entries = await _dataExchangeRepository.GetAllEntriesAsync();
                if (entries.Count == 0)
                {
                    Log.Warning("Export attempted with no journal entries found in the database.");
                    throw new AppException("Msg_NoEntriesToExportError");
                }

                var goal = await _dataExchangeRepository.GetGoalSettingValueAsync();
                if (string.IsNullOrEmpty(goal))
                {
                    Log.Warning(
                        "Export: Goal setting is missing or empty in the database. It will be exported as empty.");
                    throw new AppException("Msg_NoGoalToExportError");
                }

                using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
                using var csv = new CsvWriter(writer, _csvConfig);

                await csv.WriteRecordsAsync(entries);

                await writer.WriteLineAsync();
                await writer.WriteLineAsync("---Settings---");
                await writer.WriteLineAsync($"Goal,{goal}");

                Log.Information("Export successful: {FilePath}, {Count} entries", filePath, entries.Count);
            }
            catch (Exception ex) when (ex is not AppException)
            {
                Log.Error(ex, "Failed to export data to {FilePath}", filePath);
                throw new AppException("Msg_ExportError", isCritical: true, ex);
            }
        }

        public async Task<int> ImportFromCsvAsync(string filePath)
        {
            var (entryLines, settingsLines) = await PrepareSourceDataAsync(filePath);

            var entries = ParseEntries(entryLines);
            var goalValue = ParseGoal(settingsLines);
            if (entries.Count == 0)
            {
                Log.Warning("Import: No journal entries found in CSV file.");
                throw new AppException("Msg_EntriesIsEmptyImportError");
            }

            if (string.IsNullOrEmpty(goalValue))
            {
                Log.Warning("Import: Goal setting not found in CSV. It will be skipped.");
                throw new AppException("Msg_GoalIsEmptyImportError");
            }

            await SaveImportedDataAsync(entries, goalValue);
            return entries.Count;
        }

        private async Task<(IEnumerable<string> entries, IEnumerable<string> settings)> PrepareSourceDataAsync(
            string filePath)
        {
            if (!File.Exists(filePath)) throw new AppException("Msg_NoFIleError");

            var allLines = await File.ReadAllLinesAsync(filePath);
            if (allLines.All(l => string.IsNullOrWhiteSpace(l)) || allLines.Length == 0)
                throw new AppException("Msg_FileIsEmpty");

            int separatorIndex = Array.FindIndex(allLines, l => l.StartsWith("---Settings---"));

            var entryLines = separatorIndex != -1 ? allLines.Take(separatorIndex) : allLines;
            var settingsLines = separatorIndex != -1 ? allLines.Skip(separatorIndex + 1) : Enumerable.Empty<string>();

            return (entryLines, settingsLines);
        }

        private List<DailyCheckin> ParseEntries(IEnumerable<string> entryLines)
        {
            var entries = new List<DailyCheckin>();
            var csvContent = string.Join(Environment.NewLine, entryLines);

            using var stringReader = new StringReader(csvContent);
            using var csv = new CsvReader(stringReader, _csvConfig);

            try
            {
                foreach (var record in csv.GetRecords<DailyCheckin>())
                {
                    if (string.IsNullOrWhiteSpace(record.Description))
                        throw new AppException("Msg_EmptyDescriptionImportError", isCritical: false,
                            csv.Context.Parser.Row);

                    // record.Id = 0;
                    entries.Add(record);
                }
            }
            catch (Exception ex) when (ex is not AppException)
            {
                Log.Warning(ex, "CSV parsing error");
                throw new AppException("Msg_ImportCsvError", isCritical: false, ex);
            }

            return entries;
        }

        private string? ParseGoal(IEnumerable<string> settingsLines)
        {
            return settingsLines
                .FirstOrDefault(l => l.StartsWith("Goal,"))
                ?.Replace("Goal,", "")
                .Trim('"');
        }

        private async Task SaveImportedDataAsync(List<DailyCheckin> entries, string? goalValue)
        {
            try
            {
                await _dataExchangeRepository.ReplaceDataAsync(entries, goalValue);
            }
            catch (Exception ex)
            {
                if (ex.InnerException?.Message.Contains("UNIQUE") == true ||
                    ex.Message.Contains("UNIQUE") == true)
                {
                    Log.Warning("Import failed: Duplicate entries found in CSV.");
                    throw new AppException("Msg_DuplicateRecordsImportError");
                }

                Log.Error(ex, "DB Import Error");
                throw new AppException("Msg_DbSaveWhileImportError", isCritical: true, ex);
            }
        }
    }
}