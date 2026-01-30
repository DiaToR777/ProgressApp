using ProgressApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProgressApp.ViewModels.Today
{
    public class TodayViewModel : INotifyPropertyChanged
    {
        private readonly JournalService _service;
        private string _description;
        private DayResult _selectedResult;

        public string CurrentDate { get; } = DateTime.Today.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"));

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public DayResult SelectedResult
        {
            get => _selectedResult;
            set { _selectedResult = value; OnPropertyChanged(); }
        }
        public Array AllResults => Enum.GetValues(typeof(DayResult));
        public RelayCommand SaveCommand { get; }
        private void LoadToday()
        {
            var entry = _service.GetToday();

            if (entry != null)
            {
                Description = entry.Description;
                SelectedResult = entry.Result;
            }
            else
            {
                SelectedResult = DayResult.Relapse;
            }
        }

        public TodayViewModel(JournalService service)
        {
            _service = service;
            SaveCommand = new RelayCommand(SaveEntry);

            LoadToday(); // Загружаем данные при старте
        }
        private void SaveEntry(object? obj)
        {
            _service.SaveToday(Description, SelectedResult);
            MessageBox.Show("Запись сохранена!");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
