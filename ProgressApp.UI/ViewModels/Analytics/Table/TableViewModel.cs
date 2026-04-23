using Accessibility;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Journal;
using ProgressApp.WpfUI.Services.Message;
using Serilog;
using System.Collections.ObjectModel;

namespace ProgressApp.WpfUI.ViewModels.Analytics.Table
{
    public class TableViewModel : ViewModelBase
    {
        private readonly IJournalService _service;
        private readonly IMessageService _messageService;

        private JournalEntry? _selectedEntry;
        public ObservableCollection<JournalEntry> Entries { get; } = new();
        public JournalEntry? SelectedEntry
        {
            get => _selectedEntry;
            set => SetProperty(ref _selectedEntry, value);
        }

        public TableViewModel(IJournalService service, IMessageService messageService)
        {
            _service = service;
            _messageService = messageService;

            Log.Debug("TableViewModel: Fetching all journal entries...");
            GetEntries();
        }

        private async void GetEntries()
        {
            try
            {
                var data = await _service.GetAllEntriesAsync();

                Entries.Clear();
                foreach (var entry in data)
                {
                    Entries.Add(entry);
                }
                Log.Information("TableViewModel: Successfully loaded {Count} entries", Entries.Count);
            }
            catch (AppException ex)
            {
                Log.Error(ex, "TableViewModel: Failed to load journal entries");
                await _messageService.ShowErrorAsync(ex);
            }
        }

    }
}