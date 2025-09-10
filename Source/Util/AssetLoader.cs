using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using Newtonsoft.Json;
using UnityEngine;

namespace UncertainLuei.CaudexLib.Util
{
    public static class CaudexAssetLoader
    {
        internal static Dictionary<PluginInfo, string> modPathOverrides = [];
        
        // Overrides name of the assets folder from plugin GUID to a custom path, supports subfolders
        public static void SetAssetsDirectory(this BaseUnityPlugin plug, params string[] vals)
        {
            string output = Path.Combine(Application.streamingAssetsPath, "Modded", Path.Combine(vals));
            if (modPathOverrides.ContainsKey(plug.Info))
                modPathOverrides[plug.Info] = output;
            else
                modPathOverrides.Add(plug.Info, output);
        }

        public static void LocalizationFromFile(Language lang, string path)
            => AssetLoader.LocalizationFromFunction((target) => TryRunLocaleFunction(target, lang, () => DeserializeLocaleFromFile(path)));

        public static void LocalizationFromMod(Language lang, BaseUnityPlugin plug, params string[] paths)
        {
            string path = Path.Combine(AssetLoader.GetModPath(plug), Path.Combine(paths));
            LocalizationFromFile(lang, path);
        }

        public static void LocalizationFromFolder(Language lang, string rootPath)
        {
            foreach (string item in Directory.GetFiles(rootPath, "*.json5"))
                LocalizationFromFile(lang, item);
        }

        public static void LocalizationFromEmbeddedResource(Language lang, Assembly assembly, string path)
            => AssetLoader.LocalizationFromFunction((target) => TryRunLocaleFunction(target, lang, () => DeserializeLocaleFromEmbedded(assembly, path)));
        public static void LocalizationFromEmbeddedResource(Language lang, string path)
            => LocalizationFromEmbeddedResource(lang, Assembly.GetCallingAssembly(), path);

        // Equivalents utilising the vanilla Mystman approach
        public static void LocalizationFromEmbeddedVanilla(Language lang, Assembly assembly, string path)
            => AssetLoader.LocalizationFromFunction((target) => TryRunLocaleFunction(target, lang, () => DeserializeLocaleFromEmbedded(assembly, path)));
        public static void LocalizationFromEmbeddedVanilla(Language lang, string path)
            => LocalizationFromEmbeddedVanilla(lang, Assembly.GetCallingAssembly(), path);

        private static Dictionary<string,string> TryRunLocaleFunction(Language lang, Language setLang, Func<Dictionary<string,string>> func)
        {
            if (lang == setLang)
                return func.Invoke();
            return [];
        }
        private static Dictionary<string, string> DeserializeLocaleFromFile(string path)
            => DeserializeLocalization(File.ReadAllText(path));

        private static Dictionary<string, string> DeserializeLocaleFromEmbedded(Assembly assembly, string path)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");
            
            return DeserializeLocalization(assembly.GetManifestResourceStream(path).ToTextString());
        }

        // Vanilla localization equivalent
        private static Dictionary<string, string> DeserializeLocaleFromEmbeddedVanilla(Assembly assembly, string path)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            try
            {
                LocalizationData localizationData = JsonUtility.FromJson<LocalizationData>(assembly.GetManifestResourceStream(path).ToTextString());
                Dictionary<string, string> localizedText = [];
                foreach (LocalizationItem item in localizationData.items)
                {
                    if (localizedText.ContainsKey(item.key))
                        localizedText[item.key] = item.value;
                    else
                        localizedText.Add(item.key, item.value);
                }
                return localizedText;
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Argument \"data\" is not valid JSON/JSON5", ex);
            }
        }

        // Loads localization files using a different format than the one used in Plus.
        public static Dictionary<string,string> DeserializeLocalization(string data)
        {
            if (data.IsNullOrWhiteSpace())
                throw new ArgumentNullException("data");
            try
            {
                Dictionary<string, string> localizationData = (Dictionary<string, string>)JsonConvert.DeserializeObject(data, typeof(Dictionary<string, string>));
                return localizationData;
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Argument \"data\" is not valid JSON/JSON5", ex);
            }
        }

        public static string ToTextString(this Stream stream, bool closeStream = true)
        {
            if (stream == null)
            {
                if (closeStream) stream.Close();
                throw new ArgumentNullException("stream");
            }

            string str;
            StreamReader sr = new(stream);
            str = sr.ReadToEnd();
            
            if (closeStream)
                stream.Close();

            return str;
        }

        public static byte[] ToByteArray(this Stream stream, bool closeStream = true)
        {
            byte[] result = AssetLoader.ToByteArray(stream);
            if (closeStream)
                stream.Close();

            return result;
        }

        public static Texture2D ToTexture2D(this byte[] byteData, string name, TextureFormat format = TextureFormat.RGBA32)
        {
            Texture2D tex = new(1, 1, format, false);
            tex.LoadImage(byteData);
            tex.filterMode = FilterMode.Point;
            tex.name = name;
            return tex;
        }

        private static string GetTemporaryFile(byte[] byteData, params string[] paths)
        {
            string path = Path.Combine(Application.temporaryCachePath, Path.Combine(paths));
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using FileStream fileStream = new(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            // Write byte-array data to file if file is empty
            if (fileStream.Length == 0L)
                fileStream.Write(byteData, 0, byteData.Length);

            return path;
        }

        public static AudioClip ToAudioClip(this byte[] byteData, string name, AudioType format)
            => AssetLoader.AudioClipFromFile(GetTemporaryFile(byteData, "AudioClip", name), format);

        public static Texture2D TextureFromEmbeddedResource(string path)
            => Assembly.GetCallingAssembly().GetManifestResourceStream(path).ToByteArray().ToTexture2D(Path.GetFileNameWithoutExtension(path));
    }
}