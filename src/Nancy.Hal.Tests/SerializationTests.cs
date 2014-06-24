namespace Nancy.Hal.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using FakeItEasy;

    using Nancy.Security;

    using Newtonsoft.Json;

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
            var json = this.Serialize(config, user);

            Console.WriteLine(json);
        }

        [Fact]
        public void should_build_conditional_links()
        {
            var context = new NancyContext();
            context.CurrentUser = A.Fake<IUserIdentity>();
            A.CallTo(() => context.CurrentUser.Claims).Returns(new[] { "DeactivateUsers" });

            var user = new UserDetails() { Id = 123, Name = "Alice", Role = new RoleSummary(){Id = 456, Name = "Admin"}, IsActive = true};
            var json = this.Serialize(config, user, context);

            Console.WriteLine(json);
        }

        private string Serialize(HalJsonConfiguration config, object obj, NancyContext context = null)
        {
            if (context == null)
                context = new NancyContext();
            
            var tw = new StringWriter();
            new JsonSerializer { ContractResolver = new JsonNetHalJsonContactResolver(config, context), Formatting = Formatting.Indented}.Serialize(tw, obj);
            return tw.ToString();
        }
    }
}
