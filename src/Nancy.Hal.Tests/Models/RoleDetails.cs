namespace Nancy.Hal.Tests.Models
{
    public class RoleDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string[] Permissions { get; set; }
    }
}