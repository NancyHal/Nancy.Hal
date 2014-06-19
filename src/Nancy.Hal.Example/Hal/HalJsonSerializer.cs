namespace Nancy.Hal.Example.Hal
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class HalJsonSerializer : JsonSerializer
    {
        public HalJsonSerializer()
        {
            // required for HAL
            this.ContractResolver = new CamelCasePropertyNamesContractResolver();
            this.Formatting = Formatting.Indented;
            this.NullValueHandling = NullValueHandling.Ignore;

            // these are my personal preferences
            this.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            this.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            this.DateParseHandling = DateParseHandling.DateTimeOffset;
        }
    }
}