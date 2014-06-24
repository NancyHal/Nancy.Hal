namespace Nancy.Hal.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Nancy.Hal.Tests.Models;
    using Nancy.Responses.Negotiation;
    using Nancy.Testing;

    using Newtonsoft.Json;

    using Xunit;

    public class NancyTests
    {
        private HalJsonConfiguration config;

        public NancyTests()
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

        public class UsersModule : NancyModule
        {
            
        }

        public void can_customize_json_serializer()
        {
            // Given
            config.BuildJsonSerializer =
                () =>
                new JsonSerializer()
                    {
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        Formatting = Formatting.Indented
                    };

            var bootstrapper = new ConfigurableBootstrapper(with =>
                { 
                    with.Dependency(config);
                    with.Module<UsersModule>();
                });
            var browser = new Browser(bootstrapper);

            // When
            var result = browser.Get("/", with =>
            {
                with.HttpRequest();
                with.Accept(MediaRange.FromString("application/hal+json"));
            });

            // Then
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }

}