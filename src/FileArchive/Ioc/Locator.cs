using System;

namespace FileArchive.Ioc
{
    public class Locator : ILocator
    {
        private readonly IIocContainer _container;

        public Locator(IIocContainer container)
        {
            _container = container;
        }

        public object this[Type type] => _container.GetInstance(type);

        public object this[Type type, string key] => _container.GetInstance(type, key);

        public TInstance Get<TInstance>() => _container.GetInstance<TInstance>();

        public TInstance Get<TInstance>(string key) => _container.GetInstance<TInstance>(key);

        public TInstance Create<TInstance>() => _container.GetNewInstance<TInstance>();

        public TInstance Create<TInstance>(string key) => _container.GetNewInstance<TInstance>(key);

        public object Create(Type type) => _container.GetNewInstance(type);

        public object Create(Type type, string key) => _container.GetNewInstance(type, key);
    }
}
