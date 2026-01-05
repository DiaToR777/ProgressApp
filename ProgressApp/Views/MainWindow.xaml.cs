using System.Windows;
using ProgressApp.ViewModels;

namespace ProgressApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }
    }
}
