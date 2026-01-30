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
        public TableView()
        {
            InitializeComponent();
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
