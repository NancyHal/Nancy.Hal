namespace Nancy.Hal.Example.Model.Users.ViewModels
{
    using System;
    using System.Linq;

    public class Role
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string[] Permissions { get; set; }
    }

    public interface IRole
    {
        Guid Id { get; set; }

        string Name { get; set; }

        string[] Permissions { get; set; }
    }
}