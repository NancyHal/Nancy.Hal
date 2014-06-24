using System;

namespace Nancy.Hal.Example
{
    using System.Collections.Generic;
    using System.Linq;

    using AutoMapper;

    using Nancy.Hal.Example.Model;
    using Nancy.Hal.Example.Model.Users;
    using Nancy.Hal.Example.Model.Users.Commands;
    using Nancy.Hal.Example.Model.Users.ViewModels;
    using Nancy.Hosting.Self;
    using Nancy.Responses;

    using Ploeh.AutoFixture;

    class Program
    {
        static void Main(string[] args)
        {
            Mapper.CreateMap<UserDetails, UserSummary>();
            Mapper.CreateMap<RoleDetails, Role>();

            using (var host = new NancyHost(new Uri("http://localhost:1234")))
            {
                host.Start();
                Console.ReadLine();
            }
        }
    }

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoc.TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var db = new Database();

            var createRole = new CreateRole()
                                 {
                                     Name = "Admin",
                                     Permissions = new[] { "View Users", "Edit Users", "Deactivate Users" }
                                 };
            db.CreateRole(createRole);

            db.CreateUser(new CreateUser() { FullName = "Dan Barua", UserName = "dan", RoleId = createRole.Id.GetValueOrDefault() });

            db.CreateUser(new CreateUser() { FullName = "Jonathon Channon", UserName = "jonathon", RoleId = createRole.Id.GetValueOrDefault() });

            // let's generate some random data
            var fixture = new Fixture();
            fixture.RepeatCount = 100;

            var roles = fixture.CreateMany<CreateRole>().ToList();
            var users = fixture.CreateMany<CreateUser>().ToList();
            foreach (var r in roles)
            {
                db.CreateRole(r);
            }

            foreach (var u in users)
            {
                u.RoleId = roles.Skip(new Random().Next(0, roles.Count)).Take(1).First().Id.GetValueOrDefault(); // select random id from roles
                db.CreateUser(u);
            }

            container.Register(db);

            container.Register(BuildHypermediaConfiguration());
        }


        public static HalJsonConfiguration BuildHypermediaConfiguration()
        {
            var config = new HalJsonConfiguration();

            config.Configure<UserSummary>()
                .Link(model => new Link("self", "/users/{id}").CreateLink(model));

            config.Configure<PagedList<UserSummary>>()
                  .Embed("users", x => x.Data)
                  .Link(
                      (model, ctx) =>
                      LinkTemplates.Users.GetUsersPaged.CreateLink("self", ctx.Request.Query, new { blah = "123" }))
                  .Link(
                      (model, ctx) =>
                      LinkTemplates.Users.GetUsersPaged.CreateLink("next", ctx.Request.Query, new { page = model.PageNumber + 1 }),
                      (model) => model.PageNumber < model.TotalPages)
                  .Link(
                      (model, ctx) =>
                      LinkTemplates.Users.GetUsersPaged.CreateLink("prev", ctx.Request.Query, new { page = model.PageNumber - 1 }),
                      (model) => model.PageNumber > 0);


            config.Configure<UserDetails>()
                  .Embed("role", model => model.Role)
                  .Link(model => LinkTemplates.Users.GetUser.CreateLink("self", model))
                  .Link(model => LinkTemplates.Users.GetUser.CreateLink("edit", model))
                  .Link(model => LinkTemplates.User.ChangeRole.CreateLink(model))
                  .Link(model => LinkTemplates.User.Deactivate.CreateLink(model), model => model.Active)
                  //.Link(model => new Link("deactivate", "/users/{id}/deactivate").CreateLink(model), (model, ctx) => model.Active && ctx.CurrentUser.Claims.Any(c => c == "DeactivateUsers"))
                  .Link(model => LinkTemplates.User.Reactivate.CreateLink(model), model => !model.Active);

            config.Configure<Role>()
                .Link(model => LinkTemplates.Roles.GetRole.CreateLink("self", model));

            config.Configure<List<Role>>()
                  .Link((model, ctx) => LinkTemplates.Roles.GetRolesPaged.CreateLink("self", ctx.Request.Query));

            config.Configure<RoleDetails>()
                  .Link(model => LinkTemplates.Roles.GetRole.CreateLink("self", model))
                  .Link(model => LinkTemplates.Roles.GetRole.CreateLink("edit", model))
                  .Link(model => LinkTemplates.Roles.GetRole.CreateLink("delete", model));

            return config;
        }
    }

}
