namespace Nancy.Hal.Example.Model.Users.Commands
{
    using System;

    public class ChangeUserRole
    {
        public Guid UserId { get; set; }

        public Guid RoleId { get; set; }
    }
}