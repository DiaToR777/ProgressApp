using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Journal;
using Serilog;
using System.Collections.ObjectModel;

namespace ProgressApp.WpfUI.ViewModels.Analytics.Table
{
    public class TableViewModel : ViewModelBase
    {
        private readonly IJournalService _service;
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
            Log.Debug("TableViewModel: Fetching all journal entries...");

            try
            {
                GetEntries();
                Log.Information("TableViewModel: Successfully loaded {Count} entries", Entries.Count);

            }
            catch (AppException ex)
            {
                Log.Error(ex, "TableViewModel: Failed to load journal entries");
                messageService.ShowError(ex);
            }
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
                Log.Debug("TableViewModel: Entries refreshed. Count: {Count}", Entries.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "TableViewModel: Failed to refresh");
            }
        }

    }
}