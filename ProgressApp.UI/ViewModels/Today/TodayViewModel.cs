using ProgressApp.Core.Interfaces.IMessage;
using ProgressApp.Core.Models.Journal;
using ProgressApp.Core.Services;
using ProgressApp.WpfUI.Localization.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ProgressApp.WpfUI.ViewModels.Today
{
    public class TodayViewModel : INotifyPropertyChanged
    {
        private readonly IJournalService _service;
        private readonly IMessageService _messageService;


        private string _description;
        private DayResult _selectedResult;
        public DateOnly CurrentDate { get; } = DateOnly.FromDateTime(DateTime.Now);
        public IEnumerable<LocalizedEnum<DayResult>> AllResult { get; }

            
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }
        public DayResult SelectedResult
        {
            get => _selectedResult;
            set
            {
                if (_selectedResult == value) return;
                _selectedResult = value;
                OnPropertyChanged();
            }
        }    
        public ICommand SaveCommand { get; }

        public TodayViewModel(IJournalService service, IMessageService messageService)
        {
            _service = service;
            _messageService = messageService;


            AllResult = Enum.GetValues(typeof(DayResult))
                            .Cast<DayResult>()
                            .Select(r => new LocalizedEnum<DayResult>(r))
                            .ToList();

            SaveCommand = new RelayCommand(
                    execute: _ =>
                    {
                        try
                        {
                            SaveEntry();
                        }
                        catch (Exception ex)
                        {
                            _messageService.ShowError(ex.Message);
                        }
                    },
                    canExecute: _ => !string.IsNullOrWhiteSpace(Description)
                );
            LoadToday();
        }

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

        private void SaveEntry()
        {
            _service.SaveToday(Description, SelectedResult);
            _messageService.ShowInfo("Msg_RecordSaved");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
