using System.ComponentModel;
using ProgressApp.Domain.Models.Goals;
using ProgressApp.Domain.Models.Journal;

namespace ProgressApp.WpfUI.Localization.Helpers;

public class LocalizedEnum<T> where T : Enum
{
    public T Value { get; }
    
    public string DisplayName
    {
        get
        {
            var key = EnumLocalizationRegistry.TryGetKey(Value);

            if (key is null)
            {
                var field = Value.GetType().GetField(Value.ToString());
                var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .FirstOrDefault() as DescriptionAttribute;
                key = attribute?.Description ?? Value.ToString();
            }

            return Managers.TranslationSource.Instance[key];
        }
    }

    public LocalizedEnum(T value) => Value = value;
    public override string ToString() => DisplayName;
}
