namespace Nancy.Hal.Tests.Models
{
    public class UserDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public RoleSummary Role { get; set; }
    }
}