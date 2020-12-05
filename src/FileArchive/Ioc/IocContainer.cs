using System;
using System.Collections.Concurrent;
using System.Linq;

namespace FileArchive.Ioc
{
    public class IocContainer : IIocContainer, IDisposable
    {
        private readonly string _defaultKey = Guid.NewGuid().ToString();

        private readonly ConcurrentDictionary<(string Key, Type Interface), Delegate> _factories =
            new ConcurrentDictionary<(string Key, Type Interface), Delegate>();

        private readonly ConcurrentDictionary<(string Key, Type Interface), object> _instances =
            new ConcurrentDictionary<(string Key, Type Interface), object>();

        private readonly object _syncInstances = new object();

        public IIocContainer Register<TImplementation, TInterface>()
        {
            RegisterInternal(_defaultKey, CreateFactory<TImplementation, TInterface>());
            return this;
        }

        public IIocContainer Register<TImplementation>()
        {
            RegisterInternal(_defaultKey, CreateFactory<TImplementation, TImplementation>());
            return this;
        }

        public IIocContainer Register<TImplementation, TInterface>(string key)
        {
            RegisterInternal(key, CreateFactory<TImplementation, TInterface>());
            return this;
        }

        public IIocContainer Register<TImplementation>(string key)
        {
            RegisterInternal(key, CreateFactory<TImplementation, TImplementation>());
            return this;
        }

        public IIocContainer Register<TImplementation>(Func<TImplementation> factory)
        {
            RegisterInternal(_defaultKey, factory);
            return this;
        }

        public IIocContainer Register<TImplementation>(string key, Func<TImplementation> factory)
        {
            RegisterInternal(key, factory);
            return this;
        }

        public object GetInstance(Type type) => GetInstanceInternal(type, false, _defaultKey);

        public object GetInstance(Type type, string key) => GetInstanceInternal(type, false, key);

        public T GetInstance<T>() => (T) GetInstanceInternal(typeof(T), false, _defaultKey);

        public T GetInstance<T>(string key) => (T) GetInstanceInternal(typeof(T), false, key);

        public T GetNewInstance<T>() => (T) GetInstanceInternal(typeof(T), true, _defaultKey);

        public T GetNewInstance<T>(string key) => (T) GetInstanceInternal(typeof(T), true, key);

        public object GetNewInstance(Type type) => GetInstanceInternal(type, true, _defaultKey);

        public object GetNewInstance(Type type, string key) => GetInstanceInternal(type, true, key);

        private object GetInstanceInternal(Type type, bool createNew, string key)
        {
            var registryKey = GetKey(key, type);

            if (!_factories.TryGetValue(registryKey, out var factory))
                throw new InvalidOperationException($"Type {type.Namespace}.{type.Name} is not registered.");

            if (typeof(Delegate).IsAssignableFrom(type))
                return factory;

            if (createNew)
                return factory.DynamicInvoke();

            if (!_instances.TryGetValue(registryKey, out var instance))
            {
                lock (_syncInstances)
                {
                    if (!_instances.TryGetValue(registryKey, out instance))
                        _instances[registryKey] = instance = factory.DynamicInvoke();
                }
            }

            return instance;
        }

        private void RegisterInternal<TInterface>(string key, Func<TInterface> factory)
        {
            _factories[GetKey(key, typeof(TInterface))] = factory;
            _factories[GetKey(key, factory.GetType())] = factory;
        }

        private static (string Key, Type Interface) GetKey(string key, Type type) => (Key: key, Interface: type);

        private Func<TInterface> CreateFactory<TImplementation, TInterface>()
        {
            var ctors = typeof(TImplementation).GetConstructors();
            var ctor = ctors.FirstOrDefault(c =>
                           c.GetCustomAttributes(typeof(PreferredConstructorAttribute), false).Any())
                       ?? ctors.FirstOrDefault()
                       ?? throw new InvalidOperationException("Not an accessible public constructor found.");

            var parameters = ctor
                .GetParameters()
                .Select(info =>
                {
                    var createNew = info.GetCustomAttributes(typeof(CreateNewAttribute), false).Any();
                    var key = info
                        .GetCustomAttributes(typeof(UseKeyAttribute), false)
                        .OfType<UseKeyAttribute>()
                        .FirstOrDefault()?
                        .Key;
                    if (string.IsNullOrEmpty(key)) key = _defaultKey;
                    return new Func<object>(() => GetInstanceInternal(info.ParameterType, createNew, key));
                })
                .ToArray();

            return () =>
                (TInterface) Activator.CreateInstance(typeof(TImplementation), parameters.Select(pa => pa()).ToArray());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _instances.Clear();
                _factories.Clear();
            }
        }

        ~IocContainer()
        {
            Dispose(false);
        }
    }
}
