namespace Nancy.Hal.Example.Model.Users.ViewModels.Resources
{
    using System.Collections.Generic;

    using Nancy.Hal.Example.Hal;

    public class UserSummaryListResource : PagedRepresentationList<UserSummaryResource>
    {
        public UserSummaryListResource(IList<UserSummaryResource> res, long totalResults, long totalPages, long page, Link uriTemplate, object uriTemplateSubstitutionParams)
            : base(res, totalResults, totalPages, page, uriTemplate, uriTemplateSubstitutionParams)
        {
        }

        public override string Rel
        {
            get
            {
                return "users";
            }
        }
    }
}