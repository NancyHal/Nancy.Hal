namespace Nancy.Hal.Example.Model.Users.ViewModels
{
    using System;

    public class RoleDetails : IHaveCreatedModifiedTimestamps
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string[] Permissions { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }
    }
}