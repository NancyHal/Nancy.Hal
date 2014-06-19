namespace Nancy.Hal.Example.Model.Users.ViewModels
{
    using System;

    public class UserSummary : IUserSummary
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public bool Active { get; set; }

        public string RoleName { get; set; }
    }

    public interface IUserSummary
    {
        Guid Id { get; set; }

        string UserName { get; set; }

        string FullName { get; set; }

        string Email { get; set; }

        bool Active { get; set; }

        string RoleName { get; set; }
    }
}