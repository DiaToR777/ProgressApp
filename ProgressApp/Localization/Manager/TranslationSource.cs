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
        public string this[string key] => resManager.GetString(key, currentCulture) ?? $"[{key}]";

        //public string AppVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
        public CultureInfo CurrentCulture
        {
            get => currentCulture;
            set
            {
                if (!Equals(currentCulture, value))
                {
                    currentCulture = value;

                    Thread.CurrentThread.CurrentCulture = value;
                    Thread.CurrentThread.CurrentUICulture = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
                    OnPropertyChanged("Item[]");
                }
            }
        }
        public void ChangeLanguage(string cultureCode)
        {
            try
            {
                CurrentCulture = new CultureInfo(cultureCode);            
            }
            catch
            {
                CurrentCulture = new CultureInfo("en-US");
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
