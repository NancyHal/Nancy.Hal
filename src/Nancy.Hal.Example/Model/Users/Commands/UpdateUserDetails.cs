namespace Nancy.Hal.Example.Model.Users.Commands
{
    using System;

    public class UpdateUserDetails
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string WindowsUserName { get; set; }
    }
}