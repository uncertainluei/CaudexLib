using UnityCipher;

namespace UncertainLuei.CaudexLib.Util.Extensions
{
    /// <summary>
    /// Extensions for easier localization of strings.
    /// </summary>
    public static class LocalizationExtensions
    {
        /// <summary>
        /// Extensions for <c>CustomLevelObject</c>s and <c>CustomLevelGenerationParameter</c>s
        /// </summary>
        public static string Localize(this string key, bool encrypted = false) => LocalizationManager.Instance.GetLocalizedText(key, encrypted);
        
        public static string Localize(this string key, string fallback, bool encrypted = false)
        {
            if (LocalizationManager.Instance.localizedText.ContainsKey(key))
                return key.Localize(encrypted);
            return fallback;
        }
    }
}
