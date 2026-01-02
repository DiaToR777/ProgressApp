using ProgressApp.Services;
using ProgressApp.ViewModels.InitialSetup;
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
        public InitialSetupView()
        {
            InitializeComponent();
            DataContext = new InitialSetupViewModel();
        }

    }

}
