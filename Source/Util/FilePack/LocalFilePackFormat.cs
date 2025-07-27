using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UncertainLuei.CaudexLib.Util.FilePack
{
    public class LocalFilePackFormat(string path) : FilePackFormat
    {
        private readonly string dirPath = path;

        private List<PackedFile> _entries;

        protected override PackedFile[] GrabEntries()
        {
            _entries ??= [];
            AddEntriesFromDir(dirPath, "");

            entries = [.. _entries];
            _entries = null;
            return entries;
        }

        private void AddEntriesFromDir(string dir, string prefix)
        {
            foreach (string subDir in Directory.GetDirectories(dir))
                AddEntriesFromDir(subDir, prefix + Path.GetFileNameWithoutExtension(subDir) + "/");

            foreach (string file in Directory.GetFiles(dir))
                _entries?.Add(new LocalPackedFile(file, prefix + Path.GetFileName(file)));
        }

        public override PackedFile Get(string path)
        {
            string fullPath = Path.Combine(dirPath, path);

            if (!File.Exists(fullPath))
                return null;

            return new LocalPackedFile(fullPath, path);
        }

        public override void Reload()
        {
            _entries?.Clear();
            base.Reload();
        }
    }

    public class LocalPackedFile : PackedFile
    {
        public LocalPackedFile(string fullPath, string path)
        {
            filePath = fullPath;
            fullName = path;

            name = Path.GetFileNameWithoutExtension(fullName) + Path.GetExtension(fullName);
        }

        private readonly string filePath;
        private readonly string name;
        private readonly string fullName;

        public override string Name => name;
        public override string FullName => fullName;

        public override string ReadAllText()
        {
            return File.ReadAllText(filePath);
        }

        public override byte[] ReadAllBytes()
        {
            return File.ReadAllBytes(filePath);
        }
    }
}
