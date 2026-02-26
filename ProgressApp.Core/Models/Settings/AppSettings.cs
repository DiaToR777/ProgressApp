using System.ComponentModel.DataAnnotations;

namespace ProgressApp.Core.Models.Settings;

public class AppSettings
{
    [Key]
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
