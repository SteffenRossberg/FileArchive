using System;

namespace FileArchive.Ioc
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class UseKeyAttribute : Attribute
    {
        public string Key { get; }

        public UseKeyAttribute(string key)
        {
            Key = key;
        }
    }

}
