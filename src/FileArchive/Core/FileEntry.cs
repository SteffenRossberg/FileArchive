using System.IO;

namespace FileArchive.Core
{
    public class FileEntry
    {
        public FileEntry(FileInfo file, string basePath)
        {
            File = file;
            Name = file.Name;
            Directory = file.DirectoryName?.Remove(0, basePath.Length);
        }

        public string Name { get; }
        
        public FileInfo File { get; }

        public string Directory { get; }
    }
}