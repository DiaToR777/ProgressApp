using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Core.Data;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Journal;
using ProgressApp.Core.Models.Settings;
using Serilog;
using System.Globalization;

namespace ProgressApp.Core.Services
{
    public class DataExchangeService : IDataExchangeService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CsvConfiguration _csvConfig;

        public DataExchangeService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
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
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

                var entries = await context.Entries.AsNoTracking().ToListAsync();
                var goal = await context.Settings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Key == SettingsKeys.Goal);

                if (entries.Count == 0)
                {
                    Log.Warning("Export attempted with no journal entries found in the database.");
                    throw new AppException("Msg_NoEntriesToExportError");
                }
                if (goal == null || string.IsNullOrEmpty(goal.Value))
                {
                    Log.Warning("Export: Goal setting is missing or empty in the database. It will be exported as empty.");
                    throw new AppException("Msg_NoGoalToExportError");
                }

                using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
                using var csv = new CsvWriter(writer, _csvConfig);

                await csv.WriteRecordsAsync(entries);

                await writer.WriteLineAsync();
                await writer.WriteLineAsync("---Settings---");
                await writer.WriteLineAsync($"Goal,{goal?.Value ?? ""}");

                Log.Information("Export successful: {FilePath}, {Count} entries", filePath, entries.Count);
            }
            catch (Exception ex) when (ex is not AppException)
            {
                Log.Error(ex, "Failed to export data to {FilePath}", filePath);
                throw new AppException("Msg_ExportError", ex);
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

        private async Task<(IEnumerable<string> entries, IEnumerable<string> settings)> PrepareSourceDataAsync(string filePath)
        {
            if (!File.Exists(filePath)) throw new AppException("Msg_NoFIleError");

            var allLines = await File.ReadAllLinesAsync(filePath);
            if (allLines.All(l => string.IsNullOrWhiteSpace(l)) || allLines.Length == 0) throw new AppException("Msg_FileIsEmpty");//TODO

            int separatorIndex = Array.FindIndex(allLines, l => l.StartsWith("---Settings---"));

            var entryLines = separatorIndex != -1 ? allLines.Take(separatorIndex) : allLines;
            var settingsLines = separatorIndex != -1 ? allLines.Skip(separatorIndex + 1) : Enumerable.Empty<string>();

            return (entryLines, settingsLines);
        }

        private List<JournalEntry> ParseEntries(IEnumerable<string> entryLines)
        {
            var entries = new List<JournalEntry>();
            var csvContent = string.Join(Environment.NewLine, entryLines);

            using var stringReader = new StringReader(csvContent);
            using var csv = new CsvReader(stringReader, _csvConfig);

            try
            {
                foreach (var record in csv.GetRecords<JournalEntry>())
                {
                    if (string.IsNullOrWhiteSpace(record.Description))
                        throw new AppException("Msg_EmptyDescriptionImportError", csv.Context.Parser.Row);

                    record.Id = 0;
                    entries.Add(record);
                }
            }
            catch (Exception ex) when (ex is not AppException)
            {
                Log.Warning(ex, "CSV parsing error");
                throw new AppException("Msg_ImportCsvError", ex);
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

        private async Task SaveImportedDataAsync(List<JournalEntry> entries, string? goalValue)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                entries.ForEach(e => e.Date = e.Date.Date);
                context.Entries.RemoveRange(context.Entries);
                await context.Entries.AddRangeAsync(entries);

                if (goalValue != null)
                {
                    var setting = await context.Settings.FirstOrDefaultAsync(s => s.Key == SettingsKeys.Goal);
                    if (setting != null) setting.Value = goalValue;
                    else context.Settings.Add(new AppSettings { Key = SettingsKeys.Goal, Value = goalValue });
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (ex.InnerException?.Message.Contains("UNIQUE") == true ||
                    ex.Message.Contains("UNIQUE") == true)
                {
                    Log.Warning("Import failed: Duplicate entries found in CSV.");
                    throw new AppException("Msg_DuplicateRecordsImportError"); 
                }

                Log.Error(ex, "DB Import Error");
                throw new AppException("Msg_DbSaveWhileImportError", ex); 
            }
        }
    }
}
