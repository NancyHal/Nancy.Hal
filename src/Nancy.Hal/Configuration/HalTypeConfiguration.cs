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
        IReadOnlyDictionary<PropertyInfo, IEmbeddedResourceInfo> Embedded { get; }
    }

    public class HalTypeConfiguration<T> : IHalTypeConfiguration
    {
        private readonly Dictionary<PropertyInfo, IEmbeddedResourceInfo> embedded = new Dictionary<PropertyInfo, IEmbeddedResourceInfo>();

        private readonly List<Func<T, NancyContext, Link>> links = new List<Func<T, NancyContext, Link>>();
        private readonly object syncRoot = new object();

        public IEnumerable<Link> LinksFor(object obj, NancyContext context)
        {
            var model = (T) obj;
            return links.Select(x => x(model, context)).Where(x => x != null);
        }

        public IReadOnlyDictionary<PropertyInfo, IEmbeddedResourceInfo> Embedded
        {
            get { return embedded; }
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

        private HalTypeConfiguration<T> AddEmbeds(PropertyInfo property, IEmbeddedResourceInfo embed)
        {
            lock (syncRoot)
            {
                embedded.Add(property, embed);
            }
            return this;
        }

        public HalTypeConfiguration<T> Embeds<TEmbedded>(Expression<Func<T, TEmbedded>> property)
        {
            var propertyInfo = property.ExtractPropertyInfo();
            var getter = CreateDelegate<TEmbedded>(propertyInfo.GetMethod);
            return AddEmbeds(propertyInfo, new EmbeddedResourceInfo<T, TEmbedded>(propertyInfo.Name.ToCamelCase(), propertyInfo, getter));
        }

        public HalTypeConfiguration<T> Embeds<TEmbedded>(string rel, Expression<Func<T, TEmbedded>> property)
        {
            var propertyInfo = property.ExtractPropertyInfo();
            var getter = CreateDelegate<TEmbedded>(propertyInfo.GetMethod);
            return AddEmbeds(propertyInfo, new EmbeddedResourceInfo<T, TEmbedded>(rel, propertyInfo, getter));
        }

        private static Func<T, TEmbedded> CreateDelegate<TEmbedded>(MethodInfo methodInfo)
        {
            return obj => (TEmbedded) methodInfo.Invoke(obj, new object[0]);
        }
    }
}