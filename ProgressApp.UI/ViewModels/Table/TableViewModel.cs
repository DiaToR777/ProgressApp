using ProgressApp.Core.Models.Journal;
using ProgressApp.Core.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProgressApp.WpfUI.ViewModels.Table
{
    public class TableViewModel : INotifyPropertyChanged
    {
        private readonly IJournalService _service;
        private JournalEntry? _selectedEntry;
        public ObservableCollection<JournalEntry> Entries { get; }

        public JournalEntry? SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                if (_selectedEntry == value) return;

                _selectedEntry = value;
                OnPropertyChanged();
            }
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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

