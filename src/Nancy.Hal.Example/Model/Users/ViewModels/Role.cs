namespace Nancy.Hal.Example.Model.Users.ViewModels
{
    using System;
    using System.Linq;

    public class Role
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string[] Permissions { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Id: {0}, Name: {1}, Permissions: {2}",
                this.Id,
                this.Name,
                this.Permissions.Aggregate(string.Empty, (s, x) => s += x + ",", s => s));
        }
    }

    public interface IRole
    {
        Guid Id { get; set; }

        string Name { get; set; }

        string[] Permissions { get; set; }
    }
}