using UnityCipher;

namespace UncertainLuei.CaudexLib.Util.Extensions
{
    public static class LocalizationExtensions
    {
        public static string Localize(this string key, bool encrypted = false) => LocalizationManager.Instance.GetLocalizedText(key, encrypted);
        public static string Localize(this string key, string fallback, bool encrypted = false)
        {
            if (LocalizationManager.Instance.localizedText.ContainsKey(key))
                return key.Localize(encrypted);
            return fallback;
        }
    }
}
