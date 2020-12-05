using System.IO;

namespace FileArchive.Core
{
    public class FileEntry
    {
        public FileEntry(string sourceBasePath, string targetBasePath, string relativePath)
        {
            Source = new FileInfo(Path.Combine(sourceBasePath, relativePath));
            Target = new FileInfo(Path.Combine(targetBasePath, relativePath));
            Name = Source.Name;
            Directory = Source.DirectoryName?.Remove(0, sourceBasePath.Length);
        }

        public FileInfo Source { get; }
        public FileInfo Target { get; }
        public string Name { get; }
        public string Directory { get; }
        
        public CompareResults CompareResult { get; set; }
    }
}