using ProgressApp.WpfUI.Localization.Managers;
using System.Globalization;

namespace ProgressApp.WpfUI.Localization.Helpers
{
    public static class CultureHelper
    {
        public static string[] GetAbbreviatedDayNames()
        {
            var names = TranslationSource.Instance.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;

            var orderedNames = names.Skip(1).Concat(names.Take(1)).ToArray();

            return orderedNames
                .Select(n => char.ToUpper(n[0]) + (n.Length > 1 ? n.Substring(1).Replace(".", "") : ""))
                .ToArray();
        }
    }
}
