using ProgressApp.Data;
using ProgressApp.Model.Journal;
using ProgressApp.Model.Settings;
using ProgressApp.Views.Today;
using ProgressApp.Views.Settings;
using ProgressApp.Views.Table;


using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProgressApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new TodayView();
        }

        private void Today_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new TodayView();
        }

        private void Table_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new TableView();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new SettingsView();
        }
    }
}
