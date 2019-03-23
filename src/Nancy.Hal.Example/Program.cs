using System.IO;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Nancy.Hal.Example.Model.Users.ViewModels;

namespace Nancy.Hal.Example
{
    static class Program {
        static void Main () {

            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }

        public class DomainProfile : Profile {
            public DomainProfile () {
                CreateMap<UserDetails, UserSummary> ();
                CreateMap<RoleDetails, Role> ();
            }
        }
    }
}