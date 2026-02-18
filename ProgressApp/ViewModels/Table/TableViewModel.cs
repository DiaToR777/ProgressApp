    using ProgressApp.Models.Journal;
    using ProgressApp.Services;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    namespace ProgressApp.ViewModels.Table
    {
        public class TableViewModel : INotifyPropertyChanged
        {
            private readonly JournalService _service;
            private JournalEntry? _selectedEntry;
            public ObservableCollection<JournalEntry> Entries { get; }

            public JournalEntry? SelectedEntry
            {
                get => _selectedEntry;
                set { _selectedEntry = value; OnPropertyChanged(); }
            }
            public TableViewModel(JournalService service)
            {
                _service = service;
                var data = _service.GetAllEntries().OrderByDescending(e => e.Date);
                Entries = new ObservableCollection<JournalEntry>(data);
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

