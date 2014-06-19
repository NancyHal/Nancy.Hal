namespace Nancy.Hal.Example.Model.Users.ViewModels.Resources
{
    using System.Collections.Generic;

    using Nancy.Hal.Example.Hal;

    public class UserSummaryListDecorator : HypermediaDecorator<PagedList<UserSummary>>
    {
        protected override IEnumerable<Link> BuildHypermedia(PagedList<UserSummary> model, NancyContext context)
        {
            yield return LinkTemplates.Users.GetUsersPaged.CreateLink("self", context.Request.Query);
            yield return LinkTemplates.Users.GetUsersPaged;
        }
    }
}