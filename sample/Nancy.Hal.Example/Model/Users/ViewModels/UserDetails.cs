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

        DateTime Created { get; set; }

        DateTime? Modified { get; set; }
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

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }
    }
}