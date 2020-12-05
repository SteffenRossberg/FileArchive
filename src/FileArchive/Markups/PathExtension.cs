using System;
using System.Windows;
using System.Windows.Markup;

namespace FileArchive.Markups
{
    public class PathExtension : MarkupExtension
    {
        public string Path { get; set; }

        public object Param1 { get; set; }

        public object Param2 { get; set; }

        public PathExtension(string path, object param)
            : this(path, param, null)
        {

        }

        public PathExtension(string path, object param1, object param2)
        {
            Path = path;
            Param1 = param1;
            Param2 = param2;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new PropertyPath(Path, Param1, Param2);
        }
    }
}
