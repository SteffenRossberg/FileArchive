using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using FileArchive.Core;

namespace FileArchive.Services
{
    public interface IFileSystemService
    {
        string SelectFolder(string folder);
    }

    public class FileSystemService : IFileSystemService
    {
        public string SelectFolder(string folder)
        {
            var dlg = new FolderBrowserDialog
            {
                AutoUpgradeEnabled = true,
                Description = "Verzeichnis auswählen",
                SelectedPath = folder ?? Environment.CurrentDirectory,
                RootFolder = Environment.SpecialFolder.MyComputer,
                ShowNewFolderButton = true,
                UseDescriptionForTitle = true
            };
            return dlg.ShowDialog() == DialogResult.OK
                ? dlg.SelectedPath
                : folder;
        }
    }
}
