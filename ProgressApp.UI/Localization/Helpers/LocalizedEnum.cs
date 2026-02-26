using System.ComponentModel;

namespace ProgressApp.WpfUI.Localization.Helpers;

public class LocalizedEnum<T> where T : Enum
{
    public T Value { get; }

    public string DisplayName
    {
        get
        {
            var field = Value.GetType().GetField(Value.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                 .FirstOrDefault() as DescriptionAttribute; // Get the Description attribute if it exists

            string key = attribute?.Description ?? Value.ToString();
            return Managers.TranslationSource.Instance[key];
        }
    }

    public LocalizedEnum(T value) => Value = value;
    public override string ToString() => DisplayName;
}
