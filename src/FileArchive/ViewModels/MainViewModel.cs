using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FileArchive.Commands;
using FileArchive.Core;
using FileArchive.Ioc;
using FileArchive.Services;

namespace FileArchive.ViewModels
{
    public interface IMainViewModel : INotifyPropertyChanged
    {
        string CurrentFilter { get; set; }
        string CurrentSource { get; set; }
        string CurrentTarget { get; set; }
        ObservableCollection<string> Filters { get; }
        ObservableCollection<string> Sources { get; }
        ObservableCollection<string> Targets { get; }
        ObservableCollection<IFileViewModel> Files { get; }
        ICommand SelectSourceCommand { get; }
        ICommand SelectTargetCommand { get; }
        ICommand CompareSourceWithTargetCommand { get; }
        ICommand CopySourceToTargetCommand { get; }
        ICommand DeleteFromTargetCommand { get; }
    }

    public class MainViewModel : ViewModelBase, IMainViewModel
    {
        private string _currentFilter;
        public string CurrentFilter
        {
            get => _currentFilter;
            set => SetSetting(
                ref _currentFilter, 
                value, 
                Filters, 
                "filter",
                string.IsNullOrWhiteSpace,
                (x, y) => string.Equals(x, y, StringComparison.InvariantCultureIgnoreCase),
                v => !string.IsNullOrWhiteSpace(v));
        }

        private string _currentSource;
        public string CurrentSource
        {
            get => _currentSource;
            set => SetSetting(
                ref _currentSource,
                value,
                Sources,
                "source",
                string.IsNullOrWhiteSpace,
                (x, y) => string.Equals(x, y, StringComparison.InvariantCultureIgnoreCase),
                Directory.Exists);
        }

        private string _currentTarget;
        public string CurrentTarget
        {
            get => _currentTarget;
            set => SetSetting(
                ref _currentTarget,
                value,
                Targets,
                "target",
                string.IsNullOrWhiteSpace,
                (x, y) => string.Equals(x, y, StringComparison.InvariantCultureIgnoreCase),
                Directory.Exists);
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
        
        private ObservableCollection<IFileViewModel> _files;
        public ObservableCollection<IFileViewModel> Files
        {
            get => _files;
            set => Set(ref _files, value);
        }

        public ICommand SelectSourceCommand { get; }

        public ICommand SelectTargetCommand { get; }

        public ICommand CompareSourceWithTargetCommand { get; }

        public ICommand CopySourceToTargetCommand { get; }

        public ICommand DeleteFromTargetCommand { get; }

        public MainViewModel(
            ICommandFactory commandFactory,
            IFileSystemService fileSystemService,
            IFileService fileService,
            IConfigurationService configurationService,
            ILocator locator)
        {
            _fileSystemService = fileSystemService;
            _fileService = fileService;
            _configurationService = configurationService;
            _locator = locator;
            SelectSourceCommand = commandFactory.Create(SelectSource);
            SelectTargetCommand = commandFactory.Create(SelectTarget);
            CompareSourceWithTargetCommand = commandFactory.Create(CompareSourceWithTarget, CanCompareSourceWithTarget);
            CopySourceToTargetCommand = commandFactory.Create(CopySourceToTarget, CanCopySourceToTarget);
            DeleteFromTargetCommand = commandFactory.Create(DeleteFromTarget, CanDeleteFromTarget);
            _configurationService.Load();
            Sources = new ObservableCollection<string>(_configurationService.GetSettings("source").Select(entry => entry.Value));
            Targets = new ObservableCollection<string>(_configurationService.GetSettings("target").Select(entry => entry.Value));
            Filters = new ObservableCollection<string>(_configurationService.GetSettings("filter").Select(entry => entry.Value));
            CurrentSource = _configurationService["activesource"] ?? Sources.FirstOrDefault() ?? "";
            CurrentTarget = _configurationService["activetarget"] ?? Targets.FirstOrDefault() ?? "";
            CurrentFilter = _configurationService["activefilter"] ?? Filters.FirstOrDefault() ?? "*.*";
        }

        private void SetSetting<TValue>(
            ref TValue field, 
            TValue value, 
            ObservableCollection<TValue> settings, 
            string settingName, 
            Func<TValue, bool> isEmpty, 
            Func<TValue, TValue, bool> isEqual, 
            Func<TValue, bool> isValid,
            [CallerMemberName] string propertyName = null)
        {
            try
            {
                if (!Set(ref field, value))
                    return;
                if (!isValid(value))
                    return;
                var fieldValue = field;
                if (isEmpty(fieldValue))
                    return;
                if (isEqual(fieldValue, default))
                    return;
                if (settings.Any(setting => isEqual(setting, fieldValue)))
                    return;
                _configurationService[$"{settingName}{settings.Count}"] = fieldValue.ToString();
                settings.Add(fieldValue);
                _configurationService.Save();
            }
            finally
            {
                RaisePropertyChanged(propertyName);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void SelectSource()
            => CurrentSource = _fileSystemService.SelectFolder(CurrentSource);
        

        private void SelectTarget()
            => CurrentTarget = _fileSystemService.SelectFolder(CurrentTarget);

        private bool CanCompareSourceWithTarget()
            => !string.IsNullOrWhiteSpace(CurrentSource) 
               && !string.IsNullOrWhiteSpace(CurrentTarget);

        private void CompareSourceWithTarget()
        {
            var files = _fileService.GetFiles(CurrentSource, CurrentTarget, CurrentFilter);
            files.Sort(_fileService.ComparePaths);
            Files = new ObservableCollection<IFileViewModel>(
                files.Select(entry =>
                {
                    var fileVm = _locator.Create<IFileViewModel>();
                    fileVm.File = entry;
                    fileVm.IsSelected = entry.CompareResult == CompareResults.MissingTarget ||
                                        entry.CompareResult == CompareResults.Different;
                    return fileVm;
                }));
        }

        private bool CanCopySourceToTarget()
            => !string.IsNullOrWhiteSpace(CurrentSource) 
               && !string.IsNullOrWhiteSpace(CurrentTarget) 
               && Files?.Any(file => file.IsSelected && file.File.Source.Exists) == true;

        private void CopySourceToTarget()
            => Files
                .Where(entry => entry.IsSelected)
                .Where(entry => entry.File.Source.Exists)
                .ForEach(entry => entry.CopyToTarget());

        private bool CanDeleteFromTarget()
            => !string.IsNullOrWhiteSpace(CurrentSource) 
               && !string.IsNullOrWhiteSpace(CurrentTarget) 
               && Files?.Any(file => file.IsSelected && !file.File.Source.Exists && file.File.Target.Exists) == true;

        private void DeleteFromTarget()
        {
            Files
                .Where(entry => entry.IsSelected)
                .Where(entry => entry.File.Target.Exists)
                .ToArray()
                .ForEach(entry =>
                {
                    entry.DeleteTarget();
                    Files.Remove(entry);
                });

        }

        private readonly IFileSystemService _fileSystemService;
        private readonly IFileService _fileService;
        private readonly IConfigurationService _configurationService;
        private readonly ILocator _locator;
    }
}
