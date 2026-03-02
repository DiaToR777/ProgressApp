using ProgressApp.Core.Interfaces.IMessage;
using ProgressApp.Core.Models.Journal;
using ProgressApp.Core.Services;
using ProgressApp.WpfUI.Localization.Helpers;
using System.Windows.Input;

namespace ProgressApp.WpfUI.ViewModels.Today
{
    public class TodayViewModel : ViewModelBase
    {
        private readonly IJournalService _service;
        private readonly IMessageService _messageService;


        private string _description = string.Empty;
        private DayResult _selectedResult;
        public DateOnly CurrentDate { get; } = DateOnly.FromDateTime(DateTime.Now);
        public IEnumerable<LocalizedEnum<DayResult>> AllResult { get; }


        public string Description
        {
            get => _description;
            set
            {
                var sanitizedValue = value?.Length > 1500 ? value.Substring(0, 1500) : value;
                SetProperty(ref _description, sanitizedValue);
            }
        }
        public DayResult SelectedResult
        {
            get => _selectedResult;
            set => SetProperty(ref _selectedResult, value);

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
    }
}
