using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Core.Data;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Journal;
using ProgressApp.Core.Models.Settings;
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
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            var entries = await context.Entries.AsNoTracking().ToListAsync();
            var goal = await context.Settings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Key == SettingsKeys.Goal);

            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, _csvConfig);

            // 1. Пишем основную таблицу
            await csv.WriteRecordsAsync(entries);

            // 2. Добавляем разделитель и настройки вручную
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("---Settings---");
            await writer.WriteLineAsync($"Goal,{goal?.Value ?? ""}");
        }

        public async Task ImportFromCsvAsync(string filePath)
        {
            var entries = new List<JournalEntry>();
            string? goalValue = null;

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, _csvConfig))
            {
                // 1. Читаем записи до пустой строки или конца секции
                await foreach (var record in csv.GetRecordsAsync<JournalEntry>())
                {
                    entries.Add(record);

                    // Хак, чтобы остановиться перед секцией Settings
                    if (reader.Peek() == '-') break;
                }

                // 2. Дочитываем настройки
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.StartsWith("Goal,"))
                    {
                        goalValue = line.Replace("Goal,", "").Trim('"');
                    }
                }
            }

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            // Очистка и сохранение
            context.Entries.RemoveRange(context.Entries);
            await context.Entries.AddRangeAsync(entries);

            if (goalValue != null)
            {
                var setting = await context.Settings.FirstOrDefaultAsync(s => s.Key == SettingsKeys.Goal);
                if (setting != null) setting.Value = goalValue;
                else context.Settings.Add(new AppSettings { Key = SettingsKeys.Goal, Value = goalValue });
            }

            await context.SaveChangesAsync();
        }
    }
    
}
