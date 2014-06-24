namespace Nancy.Hal.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using FakeItEasy;

    using Nancy.Security;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    public class SerializationTests
    {
        private HalJsonConfiguration config;

        public class UserSummary
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class UserDetails
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
            public RoleSummary Role { get; set; }
        }

        public class RoleSummary
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class RoleDetails
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string[] Permissions { get; set; }
        }

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
