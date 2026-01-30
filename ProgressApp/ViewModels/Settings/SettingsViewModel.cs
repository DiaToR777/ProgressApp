using ProgressApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProgressApp.ViewModels.Settings
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settingsService;
        private string _username;
        private string _goal;

        public string Username // Має збігатися з Binding у XAML
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }
        public string Goal
        {
            get => _goal;
            set { _goal = value; OnPropertyChanged(); }
        }
        public ICommand SaveSettingsCommand { get; }
        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;

            // Завантажуємо дані при старті
            Username = _settingsService.GetUserName();
            Goal = _settingsService.GetGoal();

            SaveSettingsCommand = new RelayCommand(_ =>
            {
                try
                {
                    _settingsService.SaveSettings(Username, Goal);
                    MessageBox.Show("Налаштування збережено!");
                }
                catch (ArgumentException ex) // Ловимо тільки наші "логічні" помилки
                {
                    MessageBox.Show(ex.Message, "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Відкочуємо значення, щоб у UI було те, що реально лежить в базі
                    Username = _settingsService.GetUserName();
                    Goal = _settingsService.GetGoal();
                    return;
                }
            });
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

