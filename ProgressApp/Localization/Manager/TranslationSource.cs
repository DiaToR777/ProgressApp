using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace ProgressApp.Localization.Manager
{
    public class TranslationSource : INotifyPropertyChanged
    {
        private static readonly TranslationSource instance = new TranslationSource();
        public static TranslationSource Instance => instance;

        private readonly ResourceManager resManager = Resources.Strings.ResourceManager;
        private CultureInfo currentCulture = new CultureInfo("uk-UA");
        public string this[string key] => resManager.GetString(key, currentCulture);


        // Додай це у свій клас TranslationSource
        public string AppVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
        public void UpdateCulture(string cultureCode)
        {
            CurrentCulture = new CultureInfo(cultureCode);
        }
        public CultureInfo CurrentCulture
        {
            get => currentCulture;
            set
            {
                if (currentCulture != value)
                {
                    currentCulture = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty)); // Оновлюємо все
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
