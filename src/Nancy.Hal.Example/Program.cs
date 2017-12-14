using System;
using AutoMapper;
using Nancy.Hal.Example.Model.Users.ViewModels;
using Nancy.Hosting.Self;

namespace Nancy.Hal.Example {
    static class Program {
        static void Main () {

            using (var host = new NancyHost (new Uri ("http://localhost:1234"))) {
                host.Start ();
                Console.WriteLine ("Server running on http://localhost:1234");
                Console.ReadLine ();
            }
        }

        public class DomainProfile : Profile {
            public DomainProfile () {
                CreateMap<UserDetails, UserSummary> ();
                CreateMap<RoleDetails, Role> ();
            }
        }
    }
}