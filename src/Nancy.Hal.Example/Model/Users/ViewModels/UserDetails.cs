namespace Nancy.Hal.Example.Model.Users.ViewModels
{
    using System;

    public interface IUserDetails
    {
        Guid Id { get; set; }

        string UserName { get; set; }

        string FullName { get; set; }

        string Email { get; set; }

        string WindowsUsername { get; set; }

        bool Active { get; set; }

        DateTimeOffset Created { get; set; }

        DateTimeOffset? Modified { get; set; }
    }

    public class UserDetails : IHaveCreatedModifiedTimestamps, IUserDetails
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string WindowsUsername { get; set; }

        public bool Active { get; set; }

        public Role Role { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Modified { get; set; }
    }
}