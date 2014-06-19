namespace Nancy.Hal.Example.Model.Users.Commands
{
    using System;

    public class CreateUser
    {
        public Guid? Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string WindowsUserName { get; set; }

        public Guid RoleId { get; set; }
    }
}