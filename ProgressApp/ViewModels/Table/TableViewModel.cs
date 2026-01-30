using ProgressApp.Model.Journal;
using ProgressApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProgressApp.ViewModels.Table
{
    public class TableViewModel : INotifyPropertyChanged
    {
        private readonly JournalService _service;
        private JournalEntry? _selectedEntry;

        // Список записів для таблиці
        public ObservableCollection<JournalEntry> Entries { get; set; }

        public JournalEntry? SelectedEntry
        {
            get => _selectedEntry;
            set { _selectedEntry = value; OnPropertyChanged(); }
        }
        public TableViewModel(JournalService service)
        {
            _service = service;
            // Завантажуємо дані
            var data = _service.GetAllEntries().OrderByDescending(e => e.Date);
            Entries = new ObservableCollection<JournalEntry>(data);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

