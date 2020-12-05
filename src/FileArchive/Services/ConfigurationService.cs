using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FileArchive.Services
{
    public interface IConfigurationService : IDisposable
    {
        string this[string entryName, string defaultValue = ""] { get; set; }

        IEnumerable<KeyValuePair<string, string>> GetSettings(string settingsName);

        void Load();
        void Save();
        void Reset();
    }

    public class ConfigurationService : IConfigurationService
    {
        public string this[string entryName, string defaultValue = ""]
        {
            get => _values.TryGetValue(entryName.Trim(), out var value)
                ? value
                : defaultValue ?? string.Empty;
            set
            {
                value ??= defaultValue ?? string.Empty;
                entryName = entryName.Trim();
                _values[entryName] = value.Trim();
                _isSaved = false;
            }
        }

        public ConfigurationService()
        {
            AssemblyName name = Assembly.GetEntryAssembly()?.GetName();
            if (name == null) return;
            var configurationFileName = name.Name + ConfigurationFileExtension;
            _configurationFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configurationFileName);
            _configurationFileInfo = new FileInfo(_configurationFilePath);
            if (!_configurationFileInfo.Exists)
            {
                Reset();
            }
            else
            {
                Load();
            }
        }

        public IEnumerable<KeyValuePair<string, string>> GetSettings(string settingsName)
            => _values
                .Where(entry => Regex.IsMatch(entry.Key, $"{settingsName}[0-9]+", RegexOptions.IgnoreCase))
                .OrderBy(entry => entry.Value);

        public void Load()
        {
            _values.Clear();
            foreach (var line in File.ReadAllLines(_configurationFilePath))
            {
                if (ParseLine(line, out var key, out var value))
                    this[key, string.Empty] = value;
            }
        }

        public void Save()
        {
            Queue<string> entriesQueue = new Queue<string>();
            var parsedKeys = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var line in File.ReadAllLines(_configurationFilePath))
            {
                string entry = line;
                if (ParseLine(line, out var key, out var value))
                {
                    string rawEntry = key + KeyValueSeparator + value;
                    entry = key + KeyValueSeparator + this[key, value] + line.Substring(rawEntry.Length);
                    parsedKeys.Add(key);
                }
                entriesQueue.Enqueue(entry);
            }
            foreach (var (key, value) in _values)
            {
                if (!parsedKeys.Contains(key))
                    entriesQueue.Enqueue(key + KeyValueSeparator + value);
            }
            FileInfo configFile = new FileInfo(Path.GetTempFileName());
            using (StreamWriter writer = new StreamWriter(configFile.FullName))
            {
                while (entriesQueue.Count > 0)
                    writer.WriteLine(entriesQueue.Dequeue());
                writer.Flush();
                writer.Close();
            }
            configFile.Refresh();
            if (!configFile.Exists)
                return;
            configFile.CopyTo(_configurationFilePath, true);
            _configurationFileInfo.Refresh();
            configFile.Delete();
            _isSaved = true;
        }

        public void Reset()
        {
            if (_configurationFileInfo.Exists)
                _configurationFileInfo.Delete();
            using (StreamWriter writer = new StreamWriter(_configurationFilePath))
            {
                WriteBaseConfig(writer);
                writer.Close();
            }
            Load();
        }

        private static bool ParseLine(string line, out string key, out string value)
        {
            key = string.Empty;
            value = string.Empty;
            if (line.Trim().StartsWith(CommentStart))
                return false;
            int separatorIndex = line.IndexOf(KeyValueSeparator, StringComparison.InvariantCultureIgnoreCase);
            if (separatorIndex <= 0)
                return false;
            key = line.Substring(0, separatorIndex);
            value = line.Substring(separatorIndex + 1);
            int commentIndex = line.LastIndexOf(CommentStart, StringComparison.InvariantCultureIgnoreCase);
            if (commentIndex > 0)
                value = value.Substring(0, commentIndex);
            return true;
        }

        private static void WriteBaseConfig(StreamWriter writer)
        {
            writer.WriteLine("{0}", CommentStart);
            writer.WriteLine("{0} Dateien Archivierer Konfigurationsdatei", CommentStart);
            writer.WriteLine("{0}", CommentStart);
            writer.WriteLine("{0} Name{1}Wert {0} Kommentar", CommentStart, KeyValueSeparator);
            writer.WriteLine("{0} Kommentare werden mit \"{0}\" begonnen und können an jeder beliebigen Stelle stehen", CommentStart);
            writer.WriteLine(CommentStart);
            writer.WriteLine("{0} Filtereinstellungen:", CommentStart);
            writer.WriteLine("{0} activefilter entspricht dem zuletzt gewählten Dateifilter", CommentStart);
            writer.WriteLine("{0} filter0, filter1 .. filterX entspricht den bisher verwendeten Dateifiltern", CommentStart);
            writer.WriteLine(CommentStart);
            writer.WriteLine("{0} Quellverzeichnisse:", CommentStart);
            writer.WriteLine("{0} activesource entspricht dem zuletzt gewählten Quellverzeichnis", CommentStart);
            writer.WriteLine("{0} source0, source1 .. sourceX entspricht den bisher verwendeten Quellverzeichnissen", CommentStart);
            writer.WriteLine(CommentStart);
            writer.WriteLine("{0} Zielverzeichnisse:", CommentStart);
            writer.WriteLine("{0} activetarget entspricht dem zuletzt gewählten Zielverzeichnis", CommentStart);
            writer.WriteLine("{0} target0, target1 .. targetX entspricht den bisher verwendeten Zielverzeichnissen", CommentStart);
            writer.WriteLine(CommentStart);
            writer.WriteLine();
            writer.WriteLine("activefilter=*.*");
            writer.WriteLine("activesource=");
            writer.WriteLine("activetarget=");
            writer.WriteLine("filter0=*.*");
            writer.WriteLine("filter1=");
            writer.WriteLine("source0=");
            writer.WriteLine("target0=");
            writer.WriteLine();
            writer.Flush();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!_isSaved)
                Save();
        }

        ~ConfigurationService()
        {
            Dispose(false);
        }

        private readonly Dictionary<string, string> _values = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        private bool _isSaved = true;
        private const string ConfigurationFileExtension = ".config";
        private const string KeyValueSeparator = "=";
        private const string CommentStart = ";";
        private readonly string _configurationFilePath;
        private readonly FileInfo _configurationFileInfo;
    }
}
