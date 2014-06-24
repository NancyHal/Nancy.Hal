namespace Nancy.Hal.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using FakeItEasy;

    using Nancy.Hal.Example.Model.Users.ViewModels;
    using Nancy.Hal.ResponseProcessor;
    using Nancy.Responses;
    using Nancy.Responses.Negotiation;
    using Nancy.Security;
    using Nancy.Serialization.JsonNet;
    using Nancy.Serializers.Json.ServiceStack;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    using Xunit;

    public abstract class ResponseProcessorTests
    {
        private HalJsonConfiguration config;

        public ResponseProcessorTests()
        {
            config = Nancy.Hal.Example.Bootstrapper.BuildHypermediaConfiguration();
        }

        [Fact]
        public void test_all_the_things()
        {
            var context = new NancyContext();
            context.CurrentUser = A.Fake<IUserIdentity>();
            A.CallTo(() => context.CurrentUser.Claims).Returns(new[] { "DeactivateUsers" });

            var user = new UserDetails() { Id = Guid.NewGuid(), UserName = "Alice", Role = new Role() { Id = Guid.NewGuid(), Name = "Admin" }, Active = true };
            var json = this.Serialize(user, context);

            Assert.Equal(user.Id.ToString(), json["id"]);
            Assert.Equal("Alice", json["userName"]);
            Assert.Equal(true, json["active"]);
            Assert.Equal("/users/" + user.Id.ToString(), json["_links"]["self"]["href"]);
            Assert.Equal("/users/" + user.Id.ToString() + "/deactivate", json["_links"]["deactivate"]["href"]);
            Assert.Equal(user.Role.Id.ToString(), json["_embedded"]["role"]["id"]);
            Assert.Equal("Admin", json["_embedded"]["role"]["name"]);
            Assert.Equal("/roles/" + user.Role.Id.ToString(), json["_embedded"]["role"]["_links"]["self"]["href"]);
        }

        protected abstract ISerializer JsonSerializer { get; }

        private JObject Serialize(object model, NancyContext context = null)
        {
            if (context == null) context = new NancyContext();

            var processor = new HalJsonResponseProcessor(config, new[] { JsonSerializer });
            var response = (JsonResponse)processor.Process(MediaRange.FromString("application/hal+json"), model, context);
            var stream = new MemoryStream();
            response.Contents.Invoke(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var text = new StreamReader(stream).ReadToEnd();

            Console.WriteLine(text);
            return JObject.Parse(text);
        }
    }

    public class DefaultJsonSerializerTests : ResponseProcessorTests
    {
        protected override ISerializer JsonSerializer
        {
            get
            {
                return new DefaultJsonSerializer();
            }
        }
    }

    public class JsonNetSerializerTests : ResponseProcessorTests
    {
        protected override ISerializer JsonSerializer
        {
            get
            {
                return
                    new JsonNetSerializer(
                        new JsonSerializer()
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                Formatting = Formatting.Indented
                            });
            }
        }
    }

    public class ServiceStackSerializerTests : ResponseProcessorTests
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
