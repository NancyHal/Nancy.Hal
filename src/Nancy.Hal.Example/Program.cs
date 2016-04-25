using System;
using AutoMapper;
using Nancy.Hal.Example.Model.Users.ViewModels;
using Nancy.Hosting.Self;

namespace Nancy.Hal.Example
{
    static class Program
    {
        static void Main()
        {
            Mapper.CreateMap<UserDetails, UserSummary>();
            Mapper.CreateMap<RoleDetails, Role>();

            using (var host = new NancyHost(new Uri("http://localhost:1234")))
            {
                host.Start();
                Console.WriteLine("Server running on http://localhost:1234");
                Console.ReadLine();
            }
        }
    }
}
