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
        private readonly IMessageService _messageService;

        private JournalEntry? _selectedEntry;

        public ObservableCollection<JournalEntry> Entries { get; } = new();

        private bool _showTable;
        public bool ShowTable
        {
            get => _showTable;
            set => SetProperty(ref _showTable, value);
        }

        private bool _showEmptyState;
        public bool ShowEmptyState
        {
            get => _showEmptyState;
            set => SetProperty(ref _showEmptyState, value);
        }

        private void UpdateUIState()
        {
            if (Entries.Count > 0)
            {
                ShowTable = true;
                ShowEmptyState = false;
            }
            else
            {
                ShowTable = false;
                ShowEmptyState = true;
            }
        }
        public JournalEntry? SelectedEntry
        {
            get => _selectedEntry;
            set => SetProperty(ref _selectedEntry, value);
        }

        public TableViewModel(IJournalService service, IMessageService messageService)
        {
            _service = service;
            _messageService = messageService;
            Entries.CollectionChanged += (s, e) => UpdateUIState();

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

                UpdateUIState();
            }
            catch (AppException ex)
            {
                Log.Error(ex, "TableViewModel: Failed to load journal entries");
                await _messageService.ShowErrorAsync(ex);
            }
        }
    }
}