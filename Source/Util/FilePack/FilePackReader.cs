using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;
using BepInEx;

namespace UncertainLuei.CaudexLib.Util.FilePack
{
    public static class FilePackReader
    {
        public delegate FilePackFormat ReadCheck(string path, string ext);

        private static readonly Dictionary<PluginInfo, List<ReadCheck>> readChecks = [];
        public static void AddReadCheck(PluginInfo plugin, ReadCheck function)
        {
            if (plugin == null)
                throw new ArgumentNullException("plugin");
            if (function == null)
                throw new ArgumentNullException("function");

            if (!readChecks.TryGetValue(plugin, out List<ReadCheck> functions))
            {
                functions = [];
                readChecks.Add(plugin, functions);
            }
            functions.Add(function);
        }

        public static bool TryGrabFormat(string path, out FilePackFormat output)
        {
            return TryGrabFormat(path, Path.GetExtension(path), out output);
        }

        public static bool TryGrabFormat(string path, string extension, out FilePackFormat output)
        {
            output = null;

            foreach (List<ReadCheck> actions in readChecks.Values)
                foreach (ReadCheck action in actions)
                {
                    output = action.Invoke(path, extension);
                    if (output != null)
                        return true;
                }

            return false;
        }

        internal static void InitReadChecks(PluginInfo plugin)
        {
            // Local file paths
            AddReadCheck(plugin, (string path, string ext) =>
            {
                if (!Directory.Exists(path))
                    return null;

                return new LocalFilePackFormat(path);
            });

            // .ZIP archives
            AddReadCheck(plugin, (string path, string ext) =>
            {
                if (ext != ".zip") return null;

                ZipArchive archive;
                try
                {
                    archive = ZipFile.OpenRead(path);
                }
                catch
                {
                    return null;
                }

                return new ZipFilePackFormat(path, archive);
            });
        }
    }
}
