namespace Nancy.Hal.Example.Model.Users.ViewModels.Resources
{
    using System;

    using Nancy.Hal.Example.Hal;

    public class UserSummaryResource : Representation, IUserSummary
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public bool Active { get; set; }

        public string RoleName { get; set; }

        public override string Rel
        {
            get
            {
                return "users";
            }
            set
            {
                base.Rel = value;
            }
        }

        public override string Href
        {
            get
            {
                return "/users/" + this.Id;
            }

            set
            {
                base.Href = value;
            }
        }
        
        protected override void CreateHypermedia()
        {
            
        }
    }
}