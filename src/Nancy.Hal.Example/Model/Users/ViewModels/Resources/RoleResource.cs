namespace Nancy.Hal.Example.Model.Users.ViewModels.Resources
{
    using System;

    using Nancy.Hal.Example.Hal;

    public class RoleSummaryResource : Representation, IRole
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string[] Permissions { get; set; }

        public override string Rel
        {
            get
            {
                return "role";
            }

            set
            {
            }
        }

        public override string Href
        {
            get
            {
                return "/roles/" + this.Id;
            }

            set
            {
                base.Href = value;
            }
        }

        protected override void CreateHypermedia()
        {
            this.Links.Add(LinkTemplates.Roles.GetRolesPaged);
        }
    }
}