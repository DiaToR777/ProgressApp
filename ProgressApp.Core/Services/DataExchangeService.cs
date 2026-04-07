using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProgressApp.Core.Data;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Journal;
using ProgressApp.Core.Models.Settings;

namespace ProgressApp.Core.Services
{
    public class DataExchangeService : IDataExchangeService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DataExchangeService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public async Task ExportToCsvAsync(string filePath)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
            var entries = await context.Entries
                            .AsNoTracking()
                            .ToListAsync()
                            .ConfigureAwait(false);

            var goal = await context.Settings
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.Key == SettingsKeys.Goal)
                            .ConfigureAwait(false);

            var csvLines = new List<string> { "Id,Date,Description,Result,CreatedAt" };
            csvLines.AddRange(entries.Select(e =>
                $"{e.Id},{e.Date:O},\"{e.Description.Replace("\"", "\"\"")}\",{e.Result},{e.CreatedAt:O}"));

            csvLines.Add(""); // пустая строка разделитель
            csvLines.Add("Settings");
            csvLines.Add($"Goal,\"{(goal?.Value ?? "").Replace("\"", "\"\"")}\"");

            await File.WriteAllLinesAsync(filePath, csvLines).ConfigureAwait(false);
        }
        public async Task ImportFromCsvAsync(string filePath)
        {
            var lines = await File.ReadAllLinesAsync(filePath).ConfigureAwait(false);

            var entries = new List<JournalEntry>();
            string? goalValue = null;
            bool isSettings = false;

            foreach (var line in lines.Skip(1)) // пропускаем заголовок
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line == "Settings") { isSettings = true; continue; }

                if (isSettings)
                {
                    // парсим Goal
                    var parts = line.Split(',', 2);
                    if (parts[0] == "Goal")
                        goalValue = parts[1].Trim('"');
                }
                else
                {
                    // парсим запись
                    var parts = line.Split(',', 5);
                    entries.Add(new JournalEntry
                    {
                        Date = DateTime.Parse(parts[1]),
                        Description = parts[2].Trim('"').Replace("\"\"", "\""),
                        Result = Enum.Parse<DayResult>(parts[3]),
                        CreatedAt = DateTime.Parse(parts[4])
                    });
                }
            }

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();

            // заменяем все записи
            context.Entries.RemoveRange(context.Entries);
            await context.Entries.AddRangeAsync(entries);

            if (goalValue != null)
                await UpdateGoalAsync(context, goalValue);

            await context.SaveChangesAsync();
        }

        private async Task UpdateGoalAsync(ProgressDbContext context, string goalValue)
        {
            var setting = await context.Settings
                .FirstOrDefaultAsync(s => s.Key == SettingsKeys.Goal);

            if (setting != null)
                setting.Value = goalValue;
            else
                context.Settings.Add(new AppSettings { Key = SettingsKeys.Goal, Value = goalValue });
        }
    }
    
}
