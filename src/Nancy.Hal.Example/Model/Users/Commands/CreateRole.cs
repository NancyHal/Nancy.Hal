namespace Nancy.Hal.Example.Model.Users.Commands
{
    using System;

    public class CreateRole
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public string[] Permissions { get; set; }
    }
}