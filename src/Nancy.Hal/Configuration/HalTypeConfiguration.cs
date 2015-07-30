using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Nancy.Extensions;

namespace Nancy.Hal.Configuration
{
    public interface IHalTypeConfiguration
    {
        IEnumerable<Link> LinksFor(object model, NancyContext context);
        IEnumerable<IEmbeddedResourceInfo> Embedded();
        IEnumerable<string> Ignored();
    }

    public class AggregatingHalTypeConfiguration : IHalTypeConfiguration
    {
        private readonly IEnumerable<IHalTypeConfiguration> _delegates;

        public AggregatingHalTypeConfiguration(IEnumerable<IHalTypeConfiguration> delegates)
        {
            this._delegates = delegates;
        }

        public IEnumerable<Link> LinksFor(object model, NancyContext context)
        {
            return _delegates.SelectMany(c => c.LinksFor(model, context));
        }

        public IEnumerable<IEmbeddedResourceInfo> Embedded()
        {
            return _delegates.SelectMany(c => c.Embedded());
        }

        public IEnumerable<string> Ignored()
        {
            return _delegates.SelectMany(c => c.Ignored());
        }
    }

    public class HalTypeConfiguration<T> : IHalTypeConfiguration
    {
        private readonly IList<IEmbeddedResourceInfo> embedded = new List<IEmbeddedResourceInfo>();
        private readonly IList<Func<T, NancyContext, Link>> links = new List<Func<T, NancyContext, Link>>();
        private readonly IList<string> ignoredProperties = new List<string>();
        private readonly object syncRoot = new object();

        public IEnumerable<Link> LinksFor(object obj, NancyContext context)
        {
            var model = (T) obj;
            return links.Select(x => x(model, context)).Where(x => x != null);
        }

        public IEnumerable<IEmbeddedResourceInfo> Embedded()
        {
            return embedded;
        }

        public IEnumerable<string> Ignored()
        {
            return ignoredProperties;
        }

        private void AddLinkFn(Func<T, NancyContext, Link> getter)
        {
            lock (syncRoot)
            {
                links.Add(getter);
            }
        }
        
        public HalTypeConfiguration<T> Links(Link link)
        {
            AddLinkFn((_,__)=>link);
            return this;
        }

        public HalTypeConfiguration<T> Links(string rel, string href, string title=null)
        {
            return Links(new Link(rel, href, title));
        }

        public HalTypeConfiguration<T> Links(Func<T, Link> linkGetter)
        {
            AddLinkFn((o, ctx) => linkGetter(o));
            return this;
        }

        public HalTypeConfiguration<T> Links(Func<T, NancyContext, Link> linkGetter)
        {
            AddLinkFn(linkGetter);
            return this;
        }

        public HalTypeConfiguration<T> Links(Func<T, Link> linkGetter, Func<T, bool> predicate)
        {
            Links((model, ctx) => predicate(model) ? linkGetter(model) : null);
            return this;
        }

        public HalTypeConfiguration<T> Links(Func<T, Link> linkGetter, Func<T, NancyContext, bool> predicate)
        {
            Links((model, ctx) => predicate(model, ctx) ? linkGetter(model) : null);
            return this;
        }

        public HalTypeConfiguration<T> Links(Func<T, NancyContext, Link> linkGetter, Func<T, bool> predicate)
        {
            Links((model, ctx) => predicate(model) ? linkGetter(model, ctx) : null);
            return this;
        }

        public HalTypeConfiguration<T> Links(Func<T, NancyContext, Link> linkGetter, Func<T, NancyContext, bool> predicate)
        {
            Links((model, ctx) => predicate(model, ctx) ? linkGetter(model, ctx) : null);
            return this;
        }

        private HalTypeConfiguration<T> AddEmbeds(IEmbeddedResourceInfo embed)
        {
            lock (syncRoot)
            {
                embedded.Add(embed);
            }
            return this;
        }

        public HalTypeConfiguration<T> Embeds(Expression<Func<T, dynamic>> property)
        {
            var propName = property.ExtractPropertyInfo().Name;
            return AddEmbeds(new EmbeddedResourceInfo<T>(propName.ToCamelCase(), propName, property.Compile()));
        }

        public HalTypeConfiguration<T> Embeds(string rel, Expression<Func<T, dynamic>> property)
        {
            return AddEmbeds(new EmbeddedResourceInfo<T>(rel, property.ExtractPropertyInfo().Name, property.Compile()));
        }

        public HalTypeConfiguration<T> Projects<TEmbedded>(string rel, Expression<Func<T, TEmbedded>> property, Func<TEmbedded, dynamic> projection)
        {
            var getter = property.Compile();
            return AddEmbeds(new EmbeddedResourceInfo<T>(rel, property.ExtractPropertyInfo().Name, model => projection(getter(model))));
        }

        public HalTypeConfiguration<T> Projects<TEmbedded>(Expression<Func<T, TEmbedded>> property, Func<TEmbedded, dynamic> projection)
        {
            var getter = property.Compile();
            var propName = property.ExtractPropertyInfo().Name;
            return AddEmbeds(new EmbeddedResourceInfo<T>(propName.ToCamelCase(), propName, model => projection(getter(model))));            
        }

        public HalTypeConfiguration<T> Ignores(Expression<Func<T, dynamic>> property)
        {
            var propName = property.ExtractPropertyInfo().Name;
            return AddIgnores(propName);
        }

        private HalTypeConfiguration<T> AddIgnores(string propertyName)
        {
            lock (syncRoot)
            {
                ignoredProperties.Add(propertyName);
            }
            return this;
        }
    }
}