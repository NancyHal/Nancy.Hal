namespace Nancy.Hal.Example.Model.Users.ViewModels.Resources
{
    using System;
    using System.Collections.Generic;

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

    public class UserSummaryDecorator : HypermediaDecorator<UserSummary>
    {
        protected override IEnumerable<Link> BuildHypermedia(UserSummary model, NancyContext context)
        {
            yield return LinkTemplates.Users.GetUser.CreateLink(new { model.Id });
        }
    }

    public class UserSummaryListDecorator : HypermediaDecorator<PagedList<UserSummary>>
    {
        protected override IEnumerable<Link> BuildHypermedia(PagedList<UserSummary> model, NancyContext context)
        {
            yield return LinkTemplates.Users.GetUsersPaged.CreateLink("self", context.Request.Query);
            yield return LinkTemplates.Users.GetUsersPaged;
        }
    }
}