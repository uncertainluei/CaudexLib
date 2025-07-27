using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using MidiPlayerTK;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using Newtonsoft.Json;
using UnityEngine;

namespace UncertainLuei.CaudexLib.Util
{
    public static class CaudexAssetLoader
    {
        public static void LocalizationFromFile(Language lang, string path)
            => AssetLoader.LocalizationFromFunction((target) => TryRunLocaleFunction(target, lang, () => DeserializeLocaleFromFile(path)));

        public static void LocalizationFromEmbeddedResource(Language lang, Assembly assembly, string path)
            => AssetLoader.LocalizationFromFunction((target) => TryRunLocaleFunction(target, lang, () => DeserializeLocaleFromEmbedded(assembly, path)));
        public static void LocalizationFromEmbeddedResource(Language lang, string path)
            => LocalizationFromEmbeddedResource(lang, Assembly.GetCallingAssembly(), path);

        // Equivalents utilising the old Mystman approach
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
            
            return DeserializeLocalization(assembly.GetManifestResourceStream(path).AsString());
        }

        // Vanilla localization equivalent
        private static Dictionary<string, string> DeserializeLocaleFromEmbeddedVanilla(Assembly assembly, string path)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            try
            {
                LocalizationData localizationData = JsonUtility.FromJson<LocalizationData>(File.ReadAllText(path));
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
        private static Dictionary<string,string> DeserializeLocalization(string data)
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

        // Like AssetLoader.SpritesFromSpritesheet, but based on sprite size and sprite count
        public static Sprite[] SplitSpriteSheet(Texture2D atlas, int spriteWidth, int spriteHeight, int totalSprites = 0, float pixelsPerUnit = 100f)
        {
            int horizontalTiles = atlas.width / spriteWidth;
            int verticalTiles = atlas.height / spriteHeight;

            if (totalSprites == 0)
                totalSprites = horizontalTiles * verticalTiles;
            Sprite[] array = new Sprite[totalSprites];

            Vector2 center = Vector2.one / 2f;

            int i = 0;
            for (int y = verticalTiles - 1; y >= 0; y--)
            {
                for (int x = 0; x < horizontalTiles && i < totalSprites; x++)
                {
                    Sprite sprite = Sprite.Create(atlas, new Rect(x * spriteWidth, y * spriteHeight, spriteWidth, spriteHeight), center, pixelsPerUnit, 0u, SpriteMeshType.FullRect);
                    sprite.name = atlas.name + "_" + i;
                    array[i++] = sprite;
                }
            }
            return array;
        }

        public static byte[] AsByteArray(this Stream stream)
        {
            if (stream == null)
            {
                stream.Close();
                throw new ArgumentNullException("stream");
            }

            byte[] bytes;
            if (stream is MemoryStream ms)
            {
                bytes = ms.ToArray();
                stream.Close();
                return bytes;
            }

            MemoryStream newMs = new();
            stream.CopyTo(newMs);
            bytes = newMs.ToArray();
            stream.Close();
            newMs.Close();
            return bytes;
        }

        public static string AsString(this Stream stream)
        {
            if (stream == null)
            {
                stream.Close();
                throw new ArgumentNullException("stream");
            }

            string str;
            StreamReader sr = new(stream);
            str = sr.ReadToEnd();
            sr.Close();
            return str;
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

            using FileStream fileStream = new(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            // Write byte-array data to file if file is empty
            if (fileStream.Length == 0L)
                fileStream.Write(byteData, 0, byteData.Length);

            return path;
        }

        public static AudioClip ToAudioClip(this byte[] byteData, string name, AudioType format)
            => AssetLoader.AudioClipFromFile(GetTemporaryFile(byteData, "AudioClip", name), format);

        public static Texture2D TextureFromEmbeddedResource(string path)
            => Assembly.GetCallingAssembly().GetManifestResourceStream(path).AsByteArray().ToTexture2D(Path.GetFileNameWithoutExtension(path));
    }
}