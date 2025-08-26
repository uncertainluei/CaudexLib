using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using MTM101BaldAPI.AssetTools;

namespace UncertainLuei.CaudexLib.Util.FilePack
{
    public class ZipFilePackFormat(string path, ZipArchive archive) : FilePackFormat
    {
        ~ZipFilePackFormat()
        {
            archive?.Dispose();
        }

        private List<PackedFile> _entries;

        protected override PackedFile[] GrabEntries()
        {
            _entries ??= [];

            foreach (ZipArchiveEntry entry in archive.Entries)
                if (entry.Name != "") // Skip entries with empty names, as they're likely directories
                    _entries.Add(new ZipPackedFile(entry));

            entries = [.. _entries];
            _entries = null;
            return entries;
        }

        public override void Reload()
        {
            archive?.Dispose();
            archive = ZipFile.OpenRead(path);
            base.Reload();
        }

        public override PackedFile Get(string path)
        {
            ZipArchiveEntry entry = archive.GetEntry(path);
            return entry == null ? null : new ZipPackedFile(entry);
        }
    }

    public class ZipPackedFile(ZipArchiveEntry archiveEntry) : PackedFile
    {
        private readonly ZipArchiveEntry entry = archiveEntry;
        private readonly string name = archiveEntry.Name;
        private readonly string fullName = archiveEntry.FullName;

        public override string Name => name;
        public override string FullName => fullName;

        public override string ReadAllText() => entry.Open().ToTextString();
        public override byte[] ReadAllBytes() => entry.Open().ToByteArray();
    }
}
