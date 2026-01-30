using ProgressApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProgressApp.ViewModels.InitialSetup
{
    public class InitialSetupViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settings;


        private string _username = string.Empty;
        private string _goal = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }
        public string Goal
        {
            get => _goal;
            set
            {
                _goal = value;
                OnPropertyChanged(nameof(Goal));
            }
        }

        public ICommand FinishCommand { get; }

        public Action? Completed { get; set; }

        public InitialSetupViewModel(SettingsService settings)
        {
            _settings = settings;
            FinishCommand = new RelayCommand(_ => Finish());
        }

        private void Finish()
        {
            _settings.SaveInitial(Username, Goal);
            Completed?.Invoke();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
