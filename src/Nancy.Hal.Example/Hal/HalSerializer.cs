namespace Nancy.Hal.Example.Hal
{
    using System.Collections.Generic;
    using System.IO;

    using Nancy;
    using Nancy.Hal.Example.Hal.JsonConverters;
    using Nancy.IO;

    using Newtonsoft.Json;

    public class HalSerializer : ISerializer
    {
        private readonly JsonSerializer serializer = new HalJsonSerializer();
        private readonly ResourceConverter resourceConverter = new ResourceConverter();
        private readonly LinksConverter linksConverter = new LinksConverter();
        private readonly EmbeddedResourceConverter embeddedResourceConverter = new EmbeddedResourceConverter();

        public HalSerializer()
        {
            this.serializer.Converters.Add(this.linksConverter);
            this.serializer.Converters.Add(this.resourceConverter);
            this.serializer.Converters.Add(this.embeddedResourceConverter);
        }

        public IEnumerable<string> Extensions { get { return new string[0]; } }

        public bool CanSerialize(string contentType)
        {
            return contentType == "application/hal+json";
        }

        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            using (var writer = new JsonTextWriter(new StreamWriter(new UnclosableStreamWrapper(outputStream))))
            {
                this.serializer.Serialize(writer, model);
            }
        }
    }
}
