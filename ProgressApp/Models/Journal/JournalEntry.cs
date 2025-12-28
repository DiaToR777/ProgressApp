using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressApp.Model.Journal
{
    public class JournalEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public DayResult Result { get; set; }
        public DateTime CreatedAt { get; set; }

    }

}
