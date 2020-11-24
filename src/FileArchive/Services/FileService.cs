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

        CompareResults CompareFiles(FileEntry source, FileEntry target);

        List<FileEntry> GetFiles(string basePath, string searchPattern);
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

        public CompareResults CompareFiles(FileEntry source, FileEntry target)
        {
            source.File.Refresh();
            target.File.Refresh();
            if (source.File.Exists && !target.File.Exists)
                return CompareResults.MissingTarget;
            if (!source.File.Exists && target.File.Exists)
                return CompareResults.MissingSource;
            if (!CanReadFile(source.File))
                return CompareResults.ReadSourceFailed;
            if (!CanReadFile(target.File))
                return CompareResults.ReadTargetFailed;
            if (!CanWriteFile(target.File))
                return CompareResults.WriteTargetFailed;
            return !FilesAreEqual(source.File, target.File) 
                ? CompareResults.Different 
                : CompareResults.Equal;
        }

        public List<FileEntry> GetFiles(string basePath, string searchPattern)
            => GetSubDirectories(basePath)
                .SelectMany(directory => GetFilesOfSubDirectory(directory, searchPattern))
                .ToList();

        private static IEnumerable<FileEntry> GetFilesOfSubDirectory(string directory, string searchPattern)
        {
            try
            {
                return Directory
                    .EnumerateFiles(directory, searchPattern, SearchOption.TopDirectoryOnly)
                    .Select(file => new FileInfo(file))
                    .Select(file => new FileEntry(file, directory));
            }
            catch
            {
                return Enumerable.Empty<FileEntry>();
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
