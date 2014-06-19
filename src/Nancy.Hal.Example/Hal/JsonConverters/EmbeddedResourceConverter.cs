namespace Nancy.Hal.Example.Hal.JsonConverters
{
    using System;
    using System.Linq;

    using Newtonsoft.Json;

    public class EmbeddedResourceConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resourceList = (ILookup<string, IResource>)value;

            if (resourceList.Count == 0) ;

            writer.WriteStartObject();
            foreach (var rel in resourceList)
            {
                writer.WritePropertyName(rel.Key);
                //if (rel.Count() > 1)
                    writer.WriteStartArray();
                foreach (var res in rel)
                    serializer.Serialize(writer, res);
                //if (rel.Count() > 1)
                    writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ILookup<string, IResource>).IsAssignableFrom(objectType);
        }
    }
}