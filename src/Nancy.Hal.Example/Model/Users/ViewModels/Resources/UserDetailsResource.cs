namespace Nancy.Hal.Example.Model.Users.ViewModels.Resources
{
    using System;

    using Nancy.Hal.Example.Hal;

    public class UserDetailsResource : Representation, IUserDetails
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string WindowsUsername { get; set; }

        public bool Active { get; set; }

        public RoleSummaryResource Role { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Modified { get; set; }

        public override string Rel
        {
            get
            {
                return "user";
            }

            set
            {
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
            this.Links.Add(LinkTemplates.Users.GetUsersPaged);

            this.Links.Add(LinkTemplates.User.Edit.CreateLink(new { this.Id }));

            this.Links.Add(LinkTemplates.User.ChangePassword.CreateLink(new { this.Id }));

            //hack for now - https://github.com/JakeGinnivan/WebApi.Hal/issues/46 / https://gist.github.com/wis3guy/9427032
            this.Links.Add(new Link("change-role", "/users/" + this.Id + "/role/{roleId}"));

            if (this.Active)
            {
                this.Links.Add(LinkTemplates.User.Deactivate.CreateLink(new { this.Id }));
            }
            else
            {
                this.Links.Add(LinkTemplates.User.Reactivate.CreateLink(new { this.Id }));
            }
        }
    }
}