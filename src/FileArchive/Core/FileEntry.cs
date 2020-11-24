namespace FileArchive.Core
{
    public class FileEntry
    {
        public string FileName { get; set; }
        
        public string SourceFolder { get; set; }

        public CompareResults Result { get; set; }
    }
}