using System;

namespace Nancy.Hal.Example
{
    using System.Linq;

    using AutoMapper;

    using Nancy.Hal.Example.Model.Users;
    using Nancy.Hal.Example.Model.Users.Commands;
    using Nancy.Hal.Example.Model.Users.ViewModels;
    using Nancy.Hal.Example.Model.Users.ViewModels.Resources;
    using Nancy.Hosting.Self;

    using Ploeh.AutoFixture;

    class Program
    {
        static void Main(string[] args)
        {
            Mapper.CreateMap<UserDetails, UserSummary>();            
            Mapper.CreateMap<UserSummary, UserSummaryResource>();
            Mapper.CreateMap<UserDetails, UserDetailsResource>();
            Mapper.CreateMap<RoleDetails, Role>();
            Mapper.CreateMap<Role, RoleSummaryResource>();
            Mapper.CreateMap<RoleDetails, RoleDetailsResource>();

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
        }

        protected override void RequestStartup(TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);
            pipelines.AfterRequest.AddItemToStartOfPipeline(
                (ctx) =>
                    {
                        Console.WriteLine("BEFORE: " + ctx.Response.GetType());
                        Console.WriteLine("BEFORE: " + ctx.NegotiationContext.DefaultModel.GetType());

                        ctx.NegotiationContext.DefaultModel = new { message = "hi there!" };
                    });
            pipelines.AfterRequest.AddItemToEndOfPipeline(

                (ctx) =>
                    { Console.WriteLine("AFTER: "+ ctx.Response.GetType());
                        Console.WriteLine(ctx.Trace.TraceLog);
                    });
        }
    }
}
