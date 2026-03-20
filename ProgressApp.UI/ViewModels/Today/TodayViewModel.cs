using ProgressApp.Core.Models.Journal;
using ProgressApp.WpfUI.Localization.Helpers;
using Serilog;
using System.Windows.Input;
using ProgressApp.Core.Exceptions;
using ProgressApp.Core.Interfaces.IService;

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
                            _messageService.ShowInfo("Msg_RecordSaved");
                            Log.Debug("TodayViewModel: UI Success notification shown to user");
                        }
                        catch (AppException ex)
                        {
                            _messageService.ShowError(ex);
                        }
                    },
                    canExecute: _ => !string.IsNullOrWhiteSpace(Description)
                );
            LoadToday();
        }

        private void LoadToday()
        {
            Log.Debug("TodayViewModel: Loading data for {Date}", CurrentDate);
            try
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
            catch (AppException ex)
            {
                Log.Error(ex, "TodayViewModel: Error loading today's entry");
                _messageService.ShowError(ex); 
            }
        }

        private void SaveEntry()
        {
            Log.Information("TodayViewModel: Attempting to save entry. Result: {Result}", SelectedResult);
            try
            {
                _service.SaveToday(Description, SelectedResult);
                Log.Information("TodayViewModel: Successfully saved to database.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "TodayViewModel: Failed to save entry");
                throw;
            }
        }
    }
}
