using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileArchive.Core;

namespace FileArchive.Services
{
    public interface IFileService
    {
        int ComparePaths(FileEntry x, FileEntry y);

        CompareResults CompareFiles(FileEntry entry);

        List<FileEntry> GetFiles(string sourceBasePath, string targetBasePath, string searchPattern);

        void CopyToTarget(FileEntry entry);

        void DeleteTarget(FileEntry entry);
    }

    public class FileService : IFileService
    {
        public int ComparePaths(FileEntry x, FileEntry y)
        {
            var directoryCompareResult = string.Compare(x.Directory, y.Directory, StringComparison.InvariantCultureIgnoreCase);
            return directoryCompareResult != 0
                ? directoryCompareResult
                : string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public CompareResults CompareFiles(FileEntry entry)
        {
            entry.Source.Refresh();
            entry.Target.Refresh();
            if (entry.Source.Exists && !entry.Target.Exists)
                return CompareResults.MissingTarget;
            if (!entry.Source.Exists && entry.Target.Exists)
                return CompareResults.MissingSource;
            if (!CanReadFile(entry.Source))
                return CompareResults.ReadSourceFailed;
            if (!CanReadFile(entry.Target))
                return CompareResults.ReadTargetFailed;
            if (!CanWriteFile(entry.Target))
                return CompareResults.WriteTargetFailed;
            return !FilesAreEqual(entry.Source, entry.Target) 
                ? CompareResults.Different 
                : CompareResults.Equal;
        }

        public List<FileEntry> GetFiles(string sourceBasePath, string targetBasePath, string searchPattern)
        {
            var sourceFiles = 
                GetFiles(sourceBasePath, searchPattern)
                .Select(file => CreateFileEntry(sourceBasePath, targetBasePath, file, sourceBasePath));

            var targetFiles = 
                GetFiles(targetBasePath, searchPattern)
                .Select(file => CreateFileEntry(sourceBasePath, targetBasePath, file, targetBasePath))
                .Where(entry => !entry.Source.Exists);

            return
                sourceFiles
                    .Concat(targetFiles)
                    .Select(file =>
                    {
                        file.CompareResult = CompareFiles(file);
                        return file;
                    })
                    .ToList();
        }

        public void CopyToTarget(FileEntry entry)
        {
            if (!Directory.Exists(entry.Target.DirectoryName))
                Directory.CreateDirectory(entry.Target.DirectoryName!);
            entry.Source.CopyTo(entry.Target.FullName, true);
        }

        public void DeleteTarget(FileEntry entry)
        {
            entry.Target.Delete();
        }

        private static FileEntry CreateFileEntry(string sourcePath, string targetPath, FileSystemInfo file, string basePath)
            => new FileEntry(sourcePath, targetPath, file.FullName.Remove(0, basePath.Length + 1));

        private static IEnumerable<FileInfo> GetFiles(string path, string searchPattern)
            => GetSubDirectories(path)
                .SelectMany(directory => GetFilesOfSubDirectory(directory, searchPattern));

        private static IEnumerable<FileInfo> GetFilesOfSubDirectory(string directory, string searchPattern)
        {
            try
            {
                return Directory
                    .EnumerateFiles(directory, searchPattern, SearchOption.TopDirectoryOnly)
                    .Select(file => new FileInfo(file));
            }
            catch
            {
                return Enumerable.Empty<FileInfo>();
            }
        }

        private static IEnumerable<string> GetSubDirectories(string basePath)
        {
            var baseDirectory = new[] {basePath};
            try
            {
                return Directory
                    .EnumerateDirectories(basePath, "*", SearchOption.TopDirectoryOnly)
                    .SelectMany(GetSubDirectories)
                    .Concat(baseDirectory);
            }
            catch
            {
                return baseDirectory;
            }
        }
        
        private static bool CanReadFile(FileInfo file)
        {
            try
            {
                using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool CanWriteFile(FileInfo file)
        {
            try
            {
                using var stream = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool FilesAreEqual(FileInfo file1, FileInfo file2)
        {
            if (file1.Length != file2.Length)
                return false;
            using var stream1 = file1.OpenRead();
            using var stream2 = file1.OpenRead();
            var buffer1 = new byte[1024 * 1024 * 4].AsSpan();
            var buffer2 = new byte[1024 * 1024 * 4].AsSpan();
            int count1, count2;
            do
            {
                count1 = stream1.Read(buffer1);
                count2 = stream2.Read(buffer2);
            } while (count1 > 0
                     && count2 > 0
                     && buffer1.Slice(0, count1).SequenceEqual(buffer2.Slice(0, count2)));
            return count1 == 0 && count2 == 0;
        }
    }
}
