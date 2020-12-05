using System.ComponentModel;
using FileArchive.Core;
using FileArchive.Services;

namespace FileArchive.ViewModels
{
    public interface IFileViewModel : INotifyPropertyChanged
    {
        bool IsSelected { get; set; }

        string FileName { get; }

        string SourceDirectory { get; }

        CompareResults CompareResult { get; }

        string Result { get; }

        FileEntry File { get; set; }

        void CopyToTarget();

        void DeleteTarget();
    }

    public class FileViewModel : ViewModelBase, IFileViewModel
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        private FileEntry _file;
        public FileEntry File
        {
            get => _file;
            set
            {
                if (Set(ref _file, value))
                {
                    RaisePropertyChanged(nameof(SourceDirectory));
                    RaisePropertyChanged(nameof(CompareResult));
                }
            }
        }

        public string FileName => _file.Source.Name;

        public string SourceDirectory => _file.Source.DirectoryName;

        public CompareResults CompareResult => _file.CompareResult;

        public string Result
            => CompareResult switch
            {
                CompareResults.MissingTarget => "Zieldatei fehlt",
                CompareResults.Different => "unterschiedlich",
                CompareResults.MissingSource => "Quelldatei fehlt",
                CompareResults.ReadSourceFailed => "Quelldatei Lesezugriff verweigert",
                CompareResults.ReadTargetFailed => "Zieldatei Lesezugriff verweigert",
                CompareResults.WriteTargetFailed => "Zieldatei Schreibzugriff verweigert",
                _ => "Ok"
            };

        public FileViewModel(IFileService fileService)
        {
            _fileService = fileService;
        }

        public void CopyToTarget()
        {
            _fileService.CopyToTarget(_file);
            _file.CompareResult = _fileService.CompareFiles(_file);
            RaisePropertyChanged(nameof(CompareResult));
            RaisePropertyChanged(nameof(Result));
            IsSelected = false;
        }

        public void DeleteTarget()
        {
            _fileService.DeleteTarget(_file);
            _file.CompareResult = _fileService.CompareFiles(_file);
            RaisePropertyChanged(nameof(CompareResult));
            RaisePropertyChanged(nameof(Result));
            IsSelected = false;
        }

        private readonly IFileService _fileService;
    }
}
