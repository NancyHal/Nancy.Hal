namespace Nancy.Hal.Example.Model.Users.ViewModels.Resources
{
    using System.Collections.Generic;

    using Nancy.Hal.Example.Hal;

    public class RoleSummaryListResource : SimpleListRepresentation<RoleSummaryResource>
    {
        public RoleSummaryListResource(IList<RoleSummaryResource> resources) : base(resources)
        {
        }

        public override string Rel
        {
            get
            {
                return "roles";
            }

            set
            {
            }
        }

        public override string Href
        {
            get
            {
                return "/roles";
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