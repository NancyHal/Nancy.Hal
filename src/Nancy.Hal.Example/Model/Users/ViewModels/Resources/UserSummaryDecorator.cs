namespace Nancy.Hal.Example.Model.Users.ViewModels.Resources
{
    using System.Collections.Generic;

    using Nancy.Hal.Example.Hal;

    public class UserSummaryDecorator : HypermediaDecorator<UserSummary>
    {
        protected override IEnumerable<Link> BuildHypermedia(UserSummary model, NancyContext context)
        {
            yield return LinkTemplates.Users.GetUser.CreateLink(new { model.Id });
        }
    }
}