using System;

namespace FileArchive.Ioc
{
    public interface IIocContainer
    {
        IIocContainer Register<TImplementation>();
        IIocContainer Register<TImplementation, TInterface>();
        IIocContainer Register<TImplementation>(string key);
        IIocContainer Register<TImplementation, TInterface>(string key);
        IIocContainer Register<TImplementation>(Func<TImplementation> factory);
        IIocContainer Register<TImplementation>(string key, Func<TImplementation> factory);

        object GetInstance(Type type);
        object GetInstance(Type type, string key);
        T GetInstance<T>();
        T GetInstance<T>(string key);
        T GetNewInstance<T>();
        T GetNewInstance<T>(string key);
        object GetNewInstance(Type type);
        object GetNewInstance(Type type, string key);
    }
}
