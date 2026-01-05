using ProgressApp.Model.Journal;
using ProgressApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProgressApp.Views.Table
{
    public partial class TableView : UserControl
    {
        private JournalService _service;

        public TableView()
        {
            InitializeComponent();

            _service = new JournalService();
            DataContext = this;
            LoadTable();
        }

        public JournalEntry? SelectedEntry
        {
            get => (JournalEntry?)GetValue(SelectedEntryProperty);
            set => SetValue(SelectedEntryProperty, value);
        }

        public static readonly DependencyProperty SelectedEntryProperty =
    DependencyProperty.Register(
        nameof(SelectedEntry),
        typeof(JournalEntry),
        typeof(TableView),
        new PropertyMetadata(null)
    );


        private void Journal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedEntry = dgJournal.SelectedItem as JournalEntry;
        }

        private void LoadTable()
        {
            dgJournal.ItemsSource = _service.GetAllEntries()
                                            .OrderByDescending(e => e.Date)
                                            .ToList();
        }
        //private void LoadTable()
        //{
        //    dgJournal.ItemsSource = _service.GetAllEntries()
        //         .OrderByDescending(e => e.Date)
        //        .Select(e => new
        //        {
        //            Date = e.Date,
        //            Result = e.Result switch
        //            {
        //                DayResult.Success => "✔",
        //                DayResult.Relapse => "✖",
        //                DayResult.PartialSuccess => "➖",
        //                _ => ""
        //            }
        //        })
        //        .ToList();

        //}
    }
}
