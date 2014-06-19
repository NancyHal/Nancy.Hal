namespace Nancy.Hal.Example.Hal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    using Nancy.Hal.Example.Hal.JsonConverters;

    using Newtonsoft.Json;

    public abstract class Representation : IResource
    {
        [JsonProperty("_embedded")]
        private ILookup<string, IResource> Embedded { get; set; }

        [JsonIgnore]
        private readonly IDictionary<PropertyInfo, object> embeddedResourceProperties = new Dictionary<PropertyInfo, object>();

        protected Representation()
        {
            this.Links = new List<Link>();
        }

        [JsonIgnore]
        public virtual string Rel { get; set; }

        [JsonIgnore]
        public virtual string Href { get; set; }

        [JsonIgnore]
        public string LinkName { get; set; }

        public IList<Link> Links { get; set; }

        public void RepopulateHyperMedia()
        {
            this.CreateHypermedia();
            if (this.Links.Count(l => l.Rel == "self") == 0)
                this.Links.Insert(0, new Link { Rel = "self", Href = this.Href });
        }

        protected internal static bool IsEmbeddedResourceType(Type type)
        {
            return typeof(IResource).IsAssignableFrom(type) ||
                   typeof(IEnumerable<IResource>).IsAssignableFrom(type);
        }

        protected abstract void CreateHypermedia();

        [OnSerializing]
        private void OnSerialize(StreamingContext context)
        {
            this.RepopulateHyperMedia();

            if (ResourceConverter.IsResourceConverterContext(context))
            {
                // put all embedded resources and lists of resources into Embedded for the _embedded serializer
                var resourceList = new List<IResource>();
                foreach (var prop in this.GetType().GetProperties().Where(p => IsEmbeddedResourceType(p.PropertyType)))
                {
                    var val = prop.GetValue(this, null);
                    if (val != null)
                    {
                        // remember embedded resource property for restoring after serialization
                        this.embeddedResourceProperties.Add(prop, val);

                        // add embedded resource to collection for the serializtion
                        var res = val as IResource;
                        if (res != null)
                            resourceList.Add(res);
                        else
                            resourceList.AddRange((IEnumerable<IResource>)val);

                        // null out the embedded property so it doesn't serialize separately as a property
                        prop.SetValue(this, null, null);
                    }
                }
                foreach (var res in resourceList.Where(r => string.IsNullOrEmpty(r.Rel)))
                    res.Rel = "unknownRel-" + res.GetType().Name;
                this.Embedded = resourceList.Count > 0 ? resourceList.ToLookup(r => r.Rel) : null;
            }
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext context)
        {
            if (ResourceConverter.IsResourceConverterContext(context))
            {
                // restore embedded resource properties
                foreach (var prop in this.embeddedResourceProperties.Keys)
                    prop.SetValue(this, this.embeddedResourceProperties[prop], null);
            }
        }
    }
}