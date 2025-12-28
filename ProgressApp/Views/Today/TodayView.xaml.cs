using ProgressApp.Data;
using ProgressApp.Model.Journal;
using ProgressApp.Services;
using ProgressApp.Views.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProgressApp.Views.Today
{
    public partial class TodayView : UserControl
    {
        private JournalEntry? _todayEntry;
        private JournalService _service;


        public TodayView()
        {
            InitializeComponent();
            _service = new JournalService();

            cbResult.ItemsSource = Enum.GetValues(typeof(DayResult));


            txtDate.Text = DateTime.Today.ToString("dd MMMM yyyy",
                new CultureInfo("ru-RU"));

            LoadToday();
        }

        private void LoadToday()
        {
            _todayEntry = _service.GetToday();

            if (_todayEntry != null)
            {
                txtDescription.Text = _todayEntry.Description;
                cbResult.SelectedItem = _todayEntry.Result;
            }
            else
            {
                cbResult.SelectedItem = DayResult.Success;
            }
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {

            _service.SaveToday(
                txtDescription.Text,
                (DayResult)cbResult.SelectedItem!
            );


            MessageBox.Show("Запись сохранена!");
        }

    }


}
