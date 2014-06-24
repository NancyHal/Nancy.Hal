namespace Nancy.Hal.Example.Hal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class HypermediaRepresentationConverter : JsonConverter
    {
        private readonly IList<IHypermediaDecorator> decorators;

        private const string StreamingContextResourceConverterToken = "hal+json";

        private const StreamingContextStates StreamingContextResourceConverterState = StreamingContextStates.Other;

        public HypermediaRepresentationConverter(IEnumerable<IHypermediaDecorator> decorators)
        {
            this.decorators = decorators.ToList();
        }

        public static bool IsResourceConverterContext(StreamingContext context)
        {
            return context.Context is string && (string)context.Context == StreamingContextResourceConverterToken
                   && context.State == StreamingContextResourceConverterState;
        }

        private static StreamingContext GetResourceConverterContext()
        {
            return new StreamingContext(StreamingContextResourceConverterState, StreamingContextResourceConverterToken);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resource = (HypermediaRepresentation)value;

            //writer.WriteStartObject();

            serializer.Serialize(writer, resource.Links);

            var model = JToken.FromObject(resource.Model);
            

           // writer.WriteEndObject();

            //var saveContext = serializer.Context;
            //serializer.Context = GetResourceConverterContext();
            //serializer.Converters.Remove(this);
            //serializer.Serialize(writer, resource);
            //serializer.Converters.Add(this);
            //serializer.Context = saveContext;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(HypermediaRepresentation).IsAssignableFrom(objectType);
            //return (!IsBasicType(objectType) || typeof(IEnumerable).IsAssignableFrom(objectType));
        }

        public static bool IsBasicType(Type type)
        {
            
            return type.Namespace == "System" && (type.IsValueType || type == typeof(string));
        }
    }
}