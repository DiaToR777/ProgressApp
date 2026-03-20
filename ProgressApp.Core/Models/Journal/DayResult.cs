using System.ComponentModel;

namespace ProgressApp.Core.Models.Journal;
public enum DayResult
{
    [Description("Result_Success")]
    Success = 1,
    [Description("Result_Relapse")]
    Relapse = 0,      
    [Description("Result_Partial")]
    PartialSuccess = 2 
}