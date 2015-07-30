namespace Nancy.Hal.Example.Model.Users.Commands
{
    using System;

    public class UpdateRole
    {
        public Guid RoleId { get; set; }

        public string Name { get; set; }

        public string[] Permissions { get; set; }
    }
}