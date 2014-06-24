namespace Nancy.Hal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Nancy.Extensions;

    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    public class JsonNetHalJsonContactResolver : DefaultContractResolver
    {
        private readonly HalJsonConfiguration config;

        private readonly NancyContext context;

        public JsonNetHalJsonContactResolver(HalJsonConfiguration config, NancyContext context)
        {
            this.config = config;
            this.context = context;
        }

        protected override System.Collections.Generic.IList<JsonProperty> CreateProperties(Type type, Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            HalJsonTypeConfiguration typeConfig;
            if (!config.TryGetTypeConfiguration(type, out typeConfig))
            {
                //if (!(typeof(IHaveHalJsonLinks).IsAssignableFrom(type) || typeof(IHaveHalJsonEmbedded).IsAssignableFrom(type)))
                //    return base.CreateProperties(type, memberSerialization);
                typeConfig = new HalJsonTypeConfiguration();
            }

            var properties = new List<JsonProperty>();
            foreach (var member in this.GetSerializableMembers(type))
            {
                var property = this.CreateProperty(member, memberSerialization);
                if (typeConfig.Embedded.Any(x => x.PropertyInfo == member)) //todo: change to dictionary for quicker lookup
                {
                    property.Readable = false;
                }
                properties.Add(property);
            }

            if (typeConfig.Links.Any())
            {
                properties.Add(
                    new JsonProperty()
                    {
                        PropertyType = typeof(IDictionary<string, object>),
                        DeclaringType = type,
                        Writable = false,
                        Readable = true,
                        ValueProvider = new LinksValueProvider(typeConfig, context),
                        PropertyName = "_links"
                    });
            }
            
            if (typeConfig.Embedded.Any())
            {
                properties.Add(
                    new JsonProperty()
                    {
                        PropertyType = typeof(IDictionary<string, object>),
                        DeclaringType = type,
                        Writable = false,
                        Readable = true,
                        ValueProvider = new EmbeddedValueProvider(typeConfig),
                        PropertyName = "_embedded"
                    });
            }

            return properties;
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            propertyName = propertyName.ToCamelCase();
            return base.ResolvePropertyName(propertyName);
        }

        private class EmbeddedValueProvider : IValueProvider
        {
            private readonly HalJsonTypeConfiguration config;

            public EmbeddedValueProvider(HalJsonTypeConfiguration config)
            {
                this.config = config;
            }

            public void SetValue(object target, object value)
            {
                throw new NotSupportedException("Deserializing hal+json is not implemented.");
            }

            public object GetValue(object target)
            {
                return config.Embedded.ToDictionary(x => x.Rel, x => x.Getter.Invoke(target));
            }
        }

        public class LinksValueProvider : IValueProvider
        {
            private readonly HalJsonTypeConfiguration config;

            private readonly NancyContext nancyContext;

            public LinksValueProvider(HalJsonTypeConfiguration config, NancyContext nancyContext)
            {
                this.config = config;
                this.nancyContext = nancyContext;
            }

            public void SetValue(object target, object value)
            {
                throw new NotSupportedException("Deserializing hal+json is not implemented.");
            }

            public object GetValue(object target)
            {
                var links = config.Links.Select(x => x.Invoke(target, nancyContext)).Where(x => x != null);

                return links.ToDictionary(
                    link => link.Rel,
                    link =>
                        {
                            var linkResult = new JObject { { "href", link.Href } };
                            if (link.IsTemplated) linkResult["templated"] = true;
                            if (!string.IsNullOrEmpty(link.Title)) linkResult["title"] = link.Title;
                            return linkResult;
                        });
            }
        }
    }
}