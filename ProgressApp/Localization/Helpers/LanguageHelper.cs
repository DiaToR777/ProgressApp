using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace ProgressApp.Localization.Helpers
{
    public static class LanguageHelper
    {
        public static readonly DependencyProperty UpdateLanguageProperty =
            DependencyProperty.RegisterAttached(
                "UpdateLanguage",
                typeof(CultureInfo),
                typeof(LanguageHelper),
                new PropertyMetadata(null, OnUpdateLanguageChanged));

        public static CultureInfo GetUpdateLanguage(DependencyObject obj) => (CultureInfo)obj.GetValue(UpdateLanguageProperty);
        public static void SetUpdateLanguage(DependencyObject obj, CultureInfo value) => obj.SetValue(UpdateLanguageProperty, value);

        private static void OnUpdateLanguageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element && e.NewValue is CultureInfo culture)
            {
                element.Language = XmlLanguage.GetLanguage(culture.IetfLanguageTag);
            }
        }
    }
}
