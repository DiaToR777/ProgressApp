using System.ComponentModel;

public enum DayResult
{
    [Description("Result_Success")]
    Success = 1,
    [Description("Result_Relapse")]
    Relapse = 0,      
    [Description("Result_Partial")]
    PartialSuccess = 2 
}