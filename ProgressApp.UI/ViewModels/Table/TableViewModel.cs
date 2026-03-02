using ProgressApp.Core.Models.Journal;
using ProgressApp.Core.Services;
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
        public TableViewModel(IJournalService service)
        {
            _service = service;
            var rawData = _service.GetAllEntries().OrderByDescending(e => e.Date);

            foreach (var entry in rawData)
            {
                if (entry.Description?.Length > 150)
                {
                    entry.Description = entry.Description.Substring(0, 150);
                }
            }

            var orderedData = rawData.OrderByDescending(e => e.Date);
            Entries = new ObservableCollection<JournalEntry>(orderedData);
        }
    }
}

