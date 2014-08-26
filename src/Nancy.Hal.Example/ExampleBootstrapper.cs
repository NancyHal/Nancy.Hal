using System;
using System.Collections.Generic;
using System.Linq;
using Nancy.Hal.Configuration;
using Nancy.Hal.Example.Model;
using Nancy.Hal.Example.Model.Users;
using Nancy.Hal.Example.Model.Users.Commands;
using Nancy.Hal.Example.Model.Users.ViewModels;
using Ploeh.AutoFixture;

namespace Nancy.Hal.Example
{
    // Autodiscovered by Nancy
    public class ExampleBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoc.TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            // This is the important part - build HAL config for types
            container.Register(HypermediaConfiguration());

            var db = new Database();
            CreateTestDataIn(db);
            container.Register(db);
        }

        private static void CreateTestDataIn(Database db)
        {
            var createRole = new CreateRole
            {
                Name = "Admin",
                Permissions = new[] {"View Users", "Edit Users", "Deactivate Users"}
            };
            db.CreateRole(createRole);
            db.CreateUser(new CreateUser {FullName = "Dan Barua", UserName = "dan", RoleId = createRole.Id.GetValueOrDefault()});
            db.CreateUser(new CreateUser {FullName = "Jonathon Channon", UserName = "jonathon", RoleId = createRole.Id.GetValueOrDefault()});

            // let's generate some random data!
            var fixture = new Fixture {RepeatCount = 100};

            var roles = fixture.CreateMany<CreateRole>().ToList();
            var users = fixture.CreateMany<CreateUser>().ToList();
            foreach (var r in roles)
            {
                db.CreateRole(r);
            }

            var roleCount = roles.Count();
            foreach (var u in users)
            {
                u.RoleId = roles.Skip(new Random().Next(0, roleCount)).Take(1).First().Id.GetValueOrDefault();
                // select random id from roles
                db.CreateUser(u);
            }
        }

        private static HalConfiguration HypermediaConfiguration()
        {
            var config = new HalConfiguration();

            config.For<UserSummary>()
                .Links(model => new Link("self", "/users/{id}").CreateLink(model));

            config.For<PagedList<UserSummary>>()
                  .Embeds("users", x => x.Data)
                  .Links(
                      (model, ctx) =>
                      LinkTemplates.Users.GetUsersPaged.CreateLink("self", ctx.Request.Query, new { blah = "123" }))
                  .Links(
                      (model, ctx) =>
                      LinkTemplates.Users.GetUsersPaged.CreateLink("next", ctx.Request.Query, new { page = model.PageNumber + 1 }),
                      model => model.PageNumber < model.TotalPages)
                  .Links(
                      (model, ctx) =>
                      LinkTemplates.Users.GetUsersPaged.CreateLink("prev", ctx.Request.Query, new { page = model.PageNumber - 1 }),
                      model => model.PageNumber > 0);


            config.For<UserDetails>()
                  .Embeds("role", model => model.Role)
                  .Links(model => LinkTemplates.Users.GetUser.CreateLink("self", model))
                  .Links(model => LinkTemplates.Users.GetUser.CreateLink("edit", model))
                  .Links(model => LinkTemplates.User.ChangeRole.CreateLink(model))
                  .Links(model => LinkTemplates.User.Deactivate.CreateLink(model), model => model.Active)
                  .Links(model => LinkTemplates.User.Reactivate.CreateLink(model), model => !model.Active);

            config.For<Role>()
                .Links(model => LinkTemplates.Roles.GetRole.CreateLink("self", model));

            config.For<List<Role>>()
                  .Links((model, ctx) => LinkTemplates.Roles.GetRolesPaged.CreateLink("self", ctx.Request.Query));

            config.For<RoleDetails>()
                  .Links(model => LinkTemplates.Roles.GetRole.CreateLink("self", model))
                  .Links(model => LinkTemplates.Roles.GetRole.CreateLink("edit", model))
                  .Links(model => LinkTemplates.Roles.GetRole.CreateLink("delete", model));

            return config;
        }

    }
}