namespace Nancy.Hal.Example.Model.Users.ViewModels.Resources
{
    using System.Collections.Generic;

    using Nancy.Hal.Example.Hal;

    public class UserDetailsDecorator : HypermediaDecorator<UserDetails>
    {
        protected override IEnumerable<Link> BuildHypermedia(UserDetails model, NancyContext context)
        {
            yield return LinkTemplates.Users.GetUser.CreateLink("self", new { model.Id }); //new { model.Id });

            yield return LinkTemplates.Users.GetUsersPaged;

            yield return LinkTemplates.User.Edit.CreateLink(new { model.Id });

            yield return LinkTemplates.User.ChangePassword.CreateLink(new { model.Id });

            yield return new Link("change-role", "/users/" + model.Id + "/role/{roleId}");

            if (model.Active)
            {
                yield return LinkTemplates.User.Deactivate.CreateLink(new { model.Id });
            }
            else
            {
                yield return LinkTemplates.User.Reactivate.CreateLink(new { model.Id });
            }
        }
    }
}