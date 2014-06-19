namespace Nancy.Hal.Example.Hal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    public class LinksConverter : JsonConverter
    {
        public override bool CanRead
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var links = new HashSet<Link>((IList<Link>)value, new LinkEqualityComparer());

            writer.WriteStartObject();

            var lookup = links.ToLookup(l => l.Rel);

            foreach (var rel in lookup)
            {
                writer.WritePropertyName(rel.Key);
                if (rel.Count() > 1)
                    writer.WriteStartArray();

                foreach (var link in rel)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("href");
                    writer.WriteValue(this.ResolveUri(link.Href));

                    if (link.IsTemplated)
                    {
                        writer.WritePropertyName("templated");
                        writer.WriteValue(true);
                    }

                    writer.WriteEndObject();
                }

                if (rel.Count() > 1)
                    writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IList<Link>).IsAssignableFrom(objectType);
        }

        public string ResolveUri(string href)
        {
            //todo - figure out how to handle this
            //if (VirtualPathUtility.IsAppRelative(href))
            //    return HttpContext.Current != null ? VirtualPathUtility.ToAbsolute(href) : href.Replace("~/", "/");
            return href;
        }
    }
}