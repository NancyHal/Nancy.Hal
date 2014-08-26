using System.Linq;
using Nancy.Hal.Configuration;
using Nancy.Hal.Processors;
using Nancy.Serialization.JsonNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;
using System;
using System.IO;
using Nancy.Responses;
using Nancy.Responses.Negotiation;
using Nancy.Serializers.Json.ServiceStack;
using Newtonsoft.Json.Linq;

namespace Nancy.Hal.Tests
{

    public abstract class JsonResponseProcessorTests
    {
        [Fact]
        public void ShouldBuildStaticLinks()
        {
            var config = new HalConfiguration();
            config.For<PetOwner>().
                Links("link1", "/staticAddress1").
                Links(new Link("link2", "/staticAddress2"));
            
            var json = Serialize(new PetOwner {Name = "Bob"}, config);

            Assert.Equal("Bob", GetStringValue(json, "Name"));
            Assert.Equal("/staticAddress1", GetStringValue(json, "_links", "link1", "href"));
            Assert.Equal("/staticAddress2", GetStringValue(json, "_links", "link2", "href"));
        }

        [Fact]
        public void ShouldBuildDynamicLinks()
        {
            var config = new HalConfiguration();
            config.For<PetOwner>().
                Links(model => new Link("link1", "/dynamic/{name}").CreateLink(model)).
                Links((model, ctx) => new Link("link2", "/dynamic/{name}/{operation}").CreateLink(model, ctx.Request.Query));

            dynamic query = new DynamicDictionary();
            query.Operation = "Duck";
            var context = new NancyContext { Request = new Request("method", "path", "http") { Query = query } };
            var json = Serialize(new PetOwner { Name = "Bob" }, config, context);

            Assert.Equal("/dynamic/Bob", GetStringValue(json, "_links", "link1", "href"));
            Assert.Equal("/dynamic/Bob/Duck", GetStringValue(json, "_links", "link2", "href"));
        }

        [Fact]
        public void ShouldBuildDynamicLinksWithPredicates()
        {
            var config = new HalConfiguration();
            config.For<PetOwner>().
                Links(model => new Link("link1", "/dynamic/on/{name}").CreateLink(model), model => model.Happy).
                Links(model => new Link("link2", "/dynamic/off/{name}").CreateLink(model), (model, ctx) => !model.Happy).
                Links((model, ctx) => new Link("link3", "/dynamic/on/{name}/{operation}").CreateLink(model, ctx.Request.Query), model => model.Happy).
                Links((model, ctx) => new Link("link4", "/dynamic/off/{name}/{operation}").CreateLink(model, ctx.Request.Query), (model, ctx) => !model.Happy);

            dynamic query = new DynamicDictionary();
            query.Operation = "Duck";
            var context = new NancyContext { Request = new Request("method", "path", "http") { Query = query } };
            var json = Serialize(new PetOwner { Name = "Bob", Happy = true }, config, context);

            Assert.Equal("/dynamic/on/Bob", GetStringValue(json, "_links", "link1", "href"));
            Assert.Null(GetStringValue(json, "_links", "link2", "href"));
            Assert.Equal("/dynamic/on/Bob/Duck", GetStringValue(json, "_links", "link3", "href"));
            Assert.Null(GetStringValue(json, "_links", "link4", "href"));
        }

        [Fact]
        public void ShouldEmbedSubResources()
        {
            var config = new HalConfiguration();
            config.For<PetOwner>().
                Embeds("pampered", owner => owner.Pets).
                Embeds(owner => owner.Livestock);

            var model = new PetOwner
            {
                Name = "Bob", 
                Happy = true, 
                Pets = new []{new Animal{Type = "Cat"}}, 
                Livestock = new Animal{Type = "Chicken"}
            };
            var json = Serialize(model, config);

            Assert.Equal("Cat", GetData(json, "_embedded", "pampered")[0][AdjustName("Type")]);
            Assert.Equal("Chicken", GetStringValue(json, "_embedded", "livestock", "Type"));
        }

        private object GetStringValue(JToken json, params string[] names)
        {
            var data = GetData(json, names);
            return data!=null ? data.ToString() : null;
        }

        private JToken GetData(JToken json, params string[] names)
        {
            return names.Aggregate(json, (current, name) => current!=null ? current[AdjustName(name)] : null);
        }

        protected virtual string AdjustName(string name)
        {
            return name;
        }

        protected abstract ISerializer JsonSerializer { get; }

        private JObject Serialize(object model, IProvideHalTypeConfiguration config, NancyContext context = null)
        {
            if (context == null) context = new NancyContext();

            var processor = new HalJsonResponseProcessor(config, new[] { JsonSerializer });
            var response = (JsonResponse)processor.Process(new MediaRange("application/hal+json"), model, context);
            var stream = new MemoryStream();
            response.Contents.Invoke(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var text = new StreamReader(stream).ReadToEnd();

            Console.WriteLine(text);
            return JObject.Parse(text);
        }
    }

    public class DefaultJsonSerializerTests : JsonResponseProcessorTests
    {
        protected override ISerializer JsonSerializer
        {
            get { return new DefaultJsonSerializer(); }
        }

        // Serialiser converts names to lower case
        protected override string AdjustName(string name)
        {
            return name.ToLower();
        }
    }

    public class JsonNetSerializerTests : JsonResponseProcessorTests
    {
        protected override ISerializer JsonSerializer
        {
            get
            {
                return
                    new JsonNetSerializer(
                        new JsonSerializer
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                Formatting = Formatting.Indented
                            });
            }
        }

        // Serialiser converts names to lower case
        protected override string AdjustName(string name)
        {
            return name.ToLower();
        }
    }

    public class ServiceStackSerializerTests : JsonResponseProcessorTests
    {
        //doesnt work because ServiceStackJsonSerializer won't let me override camelcase settings
        //(need to expose a constructor so i can pass my own instance in)
        protected override ISerializer JsonSerializer
        {
            get
            {
                return new ServiceStackJsonSerializer();
            }
        }
    }
}
