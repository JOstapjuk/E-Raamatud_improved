using E_Raamatud.Resources.Localization;
using Microsoft.Maui.Storage;
using System.Globalization;

namespace E_Raamatud.Services
{
    public static class LanguageService
    {
        public static event Action? LanguageChanged;

        /// <summary>
        /// Returns the currently active language code (e.g. "en", "et", "ru").
        /// </summary>
        public static string CurrentLanguage =>
            Preferences.Get("AppLanguage", "et");

        /// <summary>
        /// Call once on app startup to restore the saved language preference.
        /// </summary>
        public static void Initialize()
        {
            var savedLang = Preferences.Get("AppLanguage", "et");
            ApplyCulture(savedLang);
        }

        /// <summary>
        /// Switch the UI language at runtime and persist the choice.
        /// </summary>
        public static void ChangeLanguage(string languageCode)
        {
            ApplyCulture(languageCode);
            Preferences.Set("AppLanguage", languageCode);
            LanguageChanged?.Invoke();
        }

        private static void ApplyCulture(string languageCode)
        {
            var culture = new CultureInfo(languageCode);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            AppResources.Culture = culture;
        }
    }
}