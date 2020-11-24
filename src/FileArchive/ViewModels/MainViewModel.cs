using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using FileArchive.Commands;
using FileArchive.Services;

namespace FileArchive.ViewModels
{
    public interface IMainViewModel
    {
        string CurrentFilter { get; set; }
        string CurrentSource { get; set; }
        string CurrentTarget { get; set; }
        ObservableCollection<string> Filters { get; }
        ObservableCollection<string> Sources { get; }
        ObservableCollection<string> Targets { get; }
        ICommand SelectSourceCommand { get; }
        ICommand SelectTargetCommand { get; }
        ICommand CompareSourceWithTargetCommand { get; }
        ICommand CopySourceToTargetCommand { get; }
        ICommand DeleteFromTargetCommand { get; }
        event PropertyChangedEventHandler PropertyChanged;
    }

    public class MainViewModel : ViewModelBase, IMainViewModel
    {
        private string _currentFilter = "*.*";
        public string CurrentFilter
        {
            get => _currentFilter;
            set => Set(ref _currentFilter, value);
        }

        private string _currentSource = "";
        public string CurrentSource
        {
            get => _currentSource;
            set => Set(ref _currentSource, value);
        }

        private string _currentTarget = "";
        public string CurrentTarget
        {
            get => _currentTarget;
            set => Set(ref _currentTarget, value);
        }

        private ObservableCollection<string> _filters;
        public ObservableCollection<string> Filters
        {
            get => _filters;
            set => Set(ref _filters, value);
        }

        private ObservableCollection<string> _sources;
        public ObservableCollection<string> Sources
        {
            get => _sources;
            set => Set(ref _sources, value);
        }

        private ObservableCollection<string> _targets;
        public ObservableCollection<string> Targets
        {
            get => _targets;
            set => Set(ref _targets, value);
        }

        public ICommand SelectSourceCommand { get; }

        public ICommand SelectTargetCommand { get; }

        public ICommand CompareSourceWithTargetCommand { get; }

        public ICommand CopySourceToTargetCommand { get; }

        public ICommand DeleteFromTargetCommand { get; }

        public MainViewModel(
            ICommandFactory commandFactory,
            IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;
            SelectSourceCommand = commandFactory.Create(SelectSource);
            SelectTargetCommand = commandFactory.Create(SelectTarget);
            CompareSourceWithTargetCommand = commandFactory.Create(CompareSourceWithTarget, CanCompareSourceWithTarget);
            CopySourceToTargetCommand = commandFactory.Create(CopySourceToTarget, CanCopySourceToTarget);
            DeleteFromTargetCommand = commandFactory.Create(DeleteFromTarget, CanDeleteFromTarget);
        }

        private void SelectSource()
            => CurrentSource = _fileSystemService.SelectFolder(CurrentSource);
        

        private void SelectTarget()
            => CurrentTarget = _fileSystemService.SelectFolder(CurrentTarget);

        private bool CanCompareSourceWithTarget()
            => !string.IsNullOrWhiteSpace(CurrentSource) && !string.IsNullOrWhiteSpace(CurrentTarget);

        private void CompareSourceWithTarget()
        {
            throw new NotImplementedException();
        }

        private bool CanCopySourceToTarget()
            => !string.IsNullOrWhiteSpace(CurrentSource) && !string.IsNullOrWhiteSpace(CurrentTarget);

        private void CopySourceToTarget()
        {
            throw new NotImplementedException();
        }

        private bool CanDeleteFromTarget()
            => !string.IsNullOrWhiteSpace(CurrentSource) && !string.IsNullOrWhiteSpace(CurrentTarget);

        private void DeleteFromTarget()
        {
            throw new NotImplementedException();
        }

        private readonly IFileSystemService _fileSystemService;
    }
}
