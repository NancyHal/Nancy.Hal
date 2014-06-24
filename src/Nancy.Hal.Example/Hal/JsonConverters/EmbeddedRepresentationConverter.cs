namespace Nancy.Hal.Example.Hal
{
    using System;
    using System.Linq;

    using Newtonsoft.Json;

    public class EmbeddedRepresentationConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resourceList = (ILookup<string, HypermediaRepresentation>)value;

            if (resourceList.Count == 0) return;

            writer.WriteStartObject();
            foreach (var rel in resourceList)
            {
                writer.WritePropertyName(rel.Key);
                writer.WriteStartArray();
                foreach (var res in rel) serializer.Serialize(writer, res);
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override object ReadJson(
            JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ILookup<string, HypermediaRepresentation>).IsAssignableFrom(objectType);
        }
    }
}