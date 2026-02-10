using ProgressApp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ProgressApp.ViewModels.Today
{
    public class TodayViewModel : INotifyPropertyChanged
    {
        private readonly JournalService _service;
        private string _description;
        private DayResult _selectedResult;

        public DateOnly CurrentDate { get; } = DateOnly.FromDateTime(DateTime.Now);

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
        public ICommand SaveCommand { get; }
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

            SaveCommand = new RelayCommand(
                    execute: _ =>
                    {
                        try
                        {
                            SaveEntry();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    },
                    canExecute: _ => !string.IsNullOrWhiteSpace(Description)
                );
            LoadToday();
        }
        private void SaveEntry()
        {
            _service.SaveToday(Description, SelectedResult);
            MessageBox.Show("Запись сохранена!");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
