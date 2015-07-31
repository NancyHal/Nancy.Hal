using System;
using System.Collections.Concurrent;
using Nancy.Hal.Configuration;

namespace Nancy.Hal.Tests
{
    public class MockTypeConfiguration : IProvideHalTypeConfiguration
    {
        private readonly ConcurrentDictionary<Type, IHalTypeConfiguration> cache = new ConcurrentDictionary<Type, IHalTypeConfiguration>();

        public IHalTypeConfiguration GetTypeConfiguration(Type type)
        {
            IHalTypeConfiguration config = null;
            cache.TryGetValue(type, out config);
            return config;
        }

        public IHalTypeConfiguration GetTypeConfiguration<T>()
        {
            return GetTypeConfiguration(typeof(T));
        }

        public void Add<T>(IHalTypeConfiguration config)
        {
            cache.TryAdd(typeof(T), config);
        }
    }
}