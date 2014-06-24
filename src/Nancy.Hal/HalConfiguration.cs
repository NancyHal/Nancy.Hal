namespace Nancy.Hal
{
    using System;
    using System.Collections.Concurrent;

    using Nancy.Hal.Configuration;

    using Newtonsoft.Json;

    public class HalJsonConfiguration
    {
        private readonly ConcurrentDictionary<Type, HalJsonTypeConfiguration> cache = new ConcurrentDictionary<Type, HalJsonTypeConfiguration>();

        public bool TryGetTypeConfiguration(Type type, out HalJsonTypeConfiguration config)
        {
            return cache.TryGetValue(type, out config);
        }

        public HalJsonTypeConfiguration<T> Configure<T>()
        {
            var config = new HalJsonTypeConfiguration<T>();
            cache.TryAdd(typeof(T), config);
            return config;
        }

        public Func<JsonSerializer> BuildJsonSerializer = () => new JsonSerializer();
    }
}