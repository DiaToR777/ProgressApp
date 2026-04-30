using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;
using ProgressApp.Core.Models.Journal;
using Serilog;

namespace ProgressApp.WpfUI.ViewModels.Analytics.Table
{
    public partial class TableViewModel : ObservableObject
    {
        private readonly IJournalService _service;
        private readonly IMessageService _messageService;

        public ObservableCollection<JournalEntry> Entries { get; } = new();

        [ObservableProperty]
        private JournalEntry? _selectedEntry;

        [ObservableProperty]
        private bool _showTable;

        [ObservableProperty]
        private bool _showEmptyState;

        [ObservableProperty]
        private bool _isBusy;

        public TableViewModel(IJournalService service, IMessageService messageService)
        {
            _service = service;
            _messageService = messageService;

            Entries.CollectionChanged += OnEntriesChanged;

            _ = LoadEntriesAsync();
        }

        private void OnEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateUIState();
        }

        private void UpdateUIState()
        {
            ShowTable = Entries.Count > 0;
            ShowEmptyState = !ShowTable;
        }

        private async Task LoadEntriesAsync()
        {
            try
            {
                IsBusy = true;

                var data = await _service.GetAllEntriesAsync();

                Entries.Clear();
                foreach (var entry in data)
                {
                    Entries.Add(entry);
                }

                Log.Debug("TableViewModel: Entries refreshed. Count: {Count}", Entries.Count);

                UpdateUIState();
            }
            catch (AppException ex)
            {
                Log.Error(ex, "TableViewModel: Failed to load journal entries");
                await _messageService.ShowErrorAsync(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}