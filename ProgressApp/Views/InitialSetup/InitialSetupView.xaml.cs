using ProgressApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProgressApp.Views.InitialSetup
{
    /// <summary>
    /// Логика взаимодействия для InitialSetupView.xaml
    /// </summary>
    public partial class InitialSetupView : UserControl
    {
        public Action? Completed;

        private readonly SettingsService _settings = new();

        public InitialSetupView()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
                return;

            _settings.SaveInitial(
                txtUsername.Text,
                txtGoal.Text
            );

            Completed?.Invoke();
        }
    }

}
