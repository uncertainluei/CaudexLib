namespace UncertainLuei.CaudexLib.Util.FilePack
{
    public abstract class FilePackFormat
    {
        protected PackedFile[] entries;

        public PackedFile[] GetAllEntries()
        {
            entries ??= GrabEntries();
            return entries;
        }

        public virtual void Reload()
        {
            entries = null;
        }

        protected abstract PackedFile[] GrabEntries();
        public abstract PackedFile Get(string path);
    }

    public abstract class PackedFile
    {
        public abstract string Name { get; }
        public abstract string FullName { get; }

        public abstract byte[] ReadAllBytes();
        public abstract string ReadAllText();
    }
}
