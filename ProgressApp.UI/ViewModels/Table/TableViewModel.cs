using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IMessage;
using ProgressApp.Core.Models.Journal;
using ProgressApp.Core.Services;
using ProgressApp.WpfUI.Services.Message;
using Serilog;
using System.Collections.ObjectModel;

namespace ProgressApp.WpfUI.ViewModels.Table
{
    public class TableViewModel : ViewModelBase
    {
        private readonly IJournalService _service;
        private JournalEntry? _selectedEntry;
        public ObservableCollection<JournalEntry> Entries { get; }

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

                var data = _service.GetAllEntries()
                                           .OrderByDescending(e => e.Date)
                                           .ToList();

                Log.Information("TableViewModel: Successfully loaded {Count} entries", data.Count);

                Entries = new ObservableCollection<JournalEntry>(data);

            }
            catch (AppException ex)
            {
                Log.Error(ex, "TableViewModel: Failed to load journal entries");
                messageService.ShowError(ex);
            }
        }
    }
}

