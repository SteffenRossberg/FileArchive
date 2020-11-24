using System;

namespace FileArchive.Ioc
{
    public interface ILocator
    {
        object this[Type type] { get; }

        object this[Type type, string key] { get; }

        TInstance Get<TInstance>();

        TInstance Get<TInstance>(string key);

        TInstance Create<TInstance>();

        TInstance Create<TInstance>(string key);

        object Create(Type type);

        object Create(Type type, string key);
    }
}
