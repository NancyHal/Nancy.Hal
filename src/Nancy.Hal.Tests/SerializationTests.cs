namespace Nancy.Hal.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;

    using FakeItEasy;

    using Nancy.Hal.Configuration;
    using Nancy.Hal.ResponseProcessor;
    using Nancy.Hal.Tests.Models;
    using Nancy.Responses;
    using Nancy.Responses.Negotiation;
    using Nancy.Security;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    public class SerializationTests
    {
        private HalJsonConfiguration config;


        public SerializationTests()
        {
            config = new HalJsonConfiguration();

            config.Configure<UserSummary>()
                .Link(model => new Link("self", "/users/{id}").CreateLink(model));

            config.Configure<List<UserSummary>>()
                  .Link((model, ctx) => new Link("self", "/users/{?query,page,pageSize}").CreateLink(ctx.Request.Query));

            config.Configure<UserDetails>()
                  .Embed("role", model => model.Role)
                  .Link(model => new Link("self", "/users/{id}").CreateLink(model))
                  .Link(model => new Link("change-role", "/users/{id}/role/{roleId}").CreateLink(model))
                  .Link(model => new Link("deactivate", "/users/{id}/deactivate").CreateLink(model), (model, ctx) => model.IsActive && ctx.CurrentUser.Claims.Any(c => c == "DeactivateUsers"))
                  .Link(model => new Link("reactivate", "/users/{id}/reactivate").CreateLink(model), model => !model.IsActive);

            config.Configure<RoleSummary>()
                .Link(model => new Link("self", "/roles/{id}").CreateLink(model));

            config.Configure<RoleDetails>()
                  .Link(model => new Link("self", "/roles/{id}").CreateLink(model))
                  .Link(model => new Link("edit", "/roles/{id}").CreateLink(model))
                  .Link(model => new Link("delete", "/roles/{id}").CreateLink(model));
        }

        [Fact]
        public void should_build_links()
        {
            var user = new UserSummary() { Id = 123, Name = "Alice" };
            var json = this.SerializeToJObject(config, user);
            Assert.Equal(123, json["id"]);
            Assert.Equal("Alice", json["name"]);
            Assert.Equal("/users/123", json["_links"]["self"]["href"]);
        }

        [Fact]
        public void should_build_conditional_links()
        {
            var context = new NancyContext();
            context.CurrentUser = A.Fake<IUserIdentity>();
            A.CallTo(() => context.CurrentUser.Claims).Returns(new[] { "DeactivateUsers" });

            var user = new UserDetails() { Id = 123, Name = "Alice", Role = new RoleSummary(){Id = 456, Name = "Admin"}, IsActive = true};
            var json = this.SerializeToJObject(config, user, context);

            Assert.Equal(123, json["id"]);
            Assert.Equal("Alice", json["name"]);
            Assert.Equal(true, json["isActive"]);
            Assert.Equal("/users/123", json["_links"]["self"]["href"]);
            Assert.Equal("/users/123/deactivate", json["_links"]["deactivate"]["href"]);
        }

        [Fact]
        public void should_embed_embedded_resources()
        {
            var context = new NancyContext();
            context.CurrentUser = A.Fake<IUserIdentity>();
            A.CallTo(() => context.CurrentUser.Claims).Returns(new[] { "DeactivateUsers" });

            var user = new UserDetails() { Id = 123, Name = "Alice", Role = new RoleSummary() { Id = 456, Name = "Admin" }, IsActive = true };
            var json = this.SerializeToJObject(config, user, context);

            Assert.Equal(456, json["_embedded"]["role"]["id"]);
            Assert.Equal("Admin", json["_embedded"]["role"]["name"]);
            Assert.Equal("/roles/456", json["_embedded"]["role"]["_links"]["self"]["href"]);
        }

        [Fact(Skip = "Broken - see https://github.com/JakeGinnivan/WebApi.Hal/issues/46")]
        public void should_leave_templated_path_params_in_href()
        {
            var context = new NancyContext();
            context.CurrentUser = A.Fake<IUserIdentity>();
            A.CallTo(() => context.CurrentUser.Claims).Returns(new[] { "DeactivateUsers" });

            var user = new UserDetails() { Id = 123, Name = "Alice", Role = new RoleSummary() { Id = 456, Name = "Admin" }, IsActive = true };
            var json = this.SerializeToJObject(config, user, context);

            Assert.Equal("/users/123/role/{roleId}", json["_links"]["change-role"]["href"]);
            Assert.Equal(true, json["_links"]["change-role"]["templated"]);
        }

        [Fact]
        public void can_serialize_wtih_dynamic()
        {
            dynamic user = new ExpandoObject();
            user._links = new Dictionary<string, Link>();
            user._embedded = new Dictionary<string, dynamic>();
            user.id = 123;
            user.name = "Alice";
            user._links.Add("self", new Link("self", "/users/{id}").CreateLink(user));

            dynamic role = new ExpandoObject();
            user._embedded["role"] = role;
            role.id = 456;
            role.name = "Admin";
            role._links = new Dictionary<string, Link>();
            role._links.Add("self", new Link("self", "/roles/{id}").CreateLink(role));

            var json = this.Serialize(config, user);
            Console.WriteLine(json);
        }

        [Fact]
        public void can_build_links_with_dynamic()
        {
            var user = new UserDetails() { Id = 123, Name = "Alice", Role = new RoleSummary() { Id = 456, Name = "Admin" }, IsActive = true };
            dynamic dynamicUser = user.ToDynamic();
            
            Console.WriteLine(dynamicUser.GetType());
            dynamicUser._links = new List<Link>();
            dynamicUser._links.Add(new Link("self", "/users/{id}").CreateLink(dynamicUser));

            dynamic role = user.Role.ToDynamic();
            Console.WriteLine(role.GetType());
            role._links = new List<Link>();
            role._links.Add(new Link("self", "/roles/{id}").CreateLink(role));
            ((IDictionary<string,Object>)dynamicUser).Remove("Role");
            dynamicUser._embedded = new Dictionary<string, dynamic>();
            dynamicUser._embedded["role"] = role;

            var json = this.Serialize(config, dynamicUser);
            Console.WriteLine(json);
        }

        [Fact]
        public void can_build_with_decorators()
        {
            var context = new NancyContext();
            context.CurrentUser = A.Fake<IUserIdentity>();
            A.CallTo(() => context.CurrentUser.Claims).Returns(new[] { "DeactivateUsers" });

            var user = new UserDetails() { Id = 123, Name = "Alice", Role = new RoleSummary(){Id = 456, Name = "Admin"}, IsActive = true};
            var processor = new HalProcessor(config, new[] { new DefaultJsonSerializer() });
            var response = (JsonResponse)processor.Process(MediaRange.FromString("application/hal+json"), user, context);

            var stream = new MemoryStream();
            response.Contents.Invoke(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var text = new StreamReader(stream).ReadToEnd();
            Console.WriteLine(text);
            var json = JObject.Parse(text);

            Assert.Equal(123, json["id"]);
            Assert.Equal("Alice", json["name"]);
            Assert.Equal(true, json["isActive"]);
            Assert.Equal("/users/123", json["_links"]["self"]["href"]);
            Assert.Equal("/users/123/deactivate", json["_links"]["deactivate"]["href"]);
            Assert.Equal(456, json["_embedded"]["role"]["id"]);
            Assert.Equal("Admin", json["_embedded"]["role"]["name"]);
            Assert.Equal("/roles/456", json["_embedded"]["role"]["_links"]["self"]["href"]);
        }

        private string Serialize(HalJsonConfiguration config, object obj, NancyContext context = null)
        {
            if (context == null) context = new NancyContext();
            
            var textWriter = new StringWriter();
            new JsonSerializer
                {
                    ContractResolver = new JsonNetHalJsonContactResolver(config, context),
                    Formatting = Formatting.Indented
                }.Serialize(textWriter, obj);

            return textWriter.ToString();
        }

        private JObject SerializeToJObject(HalJsonConfiguration config, object obj, NancyContext context = null)
        {
            return JObject.Parse(Serialize(config, obj, context));
        }
    }
}
