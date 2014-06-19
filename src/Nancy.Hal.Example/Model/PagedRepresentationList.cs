namespace Nancy.Hal.Example.Model
{
    using System.Collections.Generic;

    using Nancy.Hal.Example.Hal;

    public abstract class PagedRepresentationList<TRepresentation> : SimpleListRepresentation<TRepresentation> where TRepresentation : Representation
    {
        protected object UriTemplateSubstitutionParams;

        private readonly Link uriTemplate;

        protected PagedRepresentationList(IList<TRepresentation> res, long totalResults, long totalPages, long page, Link uriTemplate, object uriTemplateSubstitutionParams)
            : base(res)
        {
            this.uriTemplate = uriTemplate;
            this.TotalResults = totalResults;
            this.TotalPages = totalPages;
            this.Page = page;
            this.UriTemplateSubstitutionParams = uriTemplateSubstitutionParams;
        }

        public long TotalResults { get; set; }

        public long TotalPages { get; set; }

        public long Page { get; set; }


        protected override void CreateHypermedia()
        {
            var prms = new List<object> { new { page = this.Page } };
            if (this.UriTemplateSubstitutionParams != null)
                prms.Add(this.UriTemplateSubstitutionParams);

            this.Href = this.Href ?? this.uriTemplate.CreateLink(prms.ToArray()).Href;

            this.Links.Add(new Link { Href = this.Href, Rel = "self" });

            if (this.Page > 0)
            {
                var item = this.UriTemplateSubstitutionParams == null
                               ? this.uriTemplate.CreateLink("prev", new { Page = this.Page - 1 })
                               : this.uriTemplate.CreateLink("prev", this.UriTemplateSubstitutionParams, new { Page = this.Page - 1 }); // page overrides UriTemplateSubstitutionParams
                this.Links.Add(item);
            }

            if (this.Page > 0 && this.Page < this.TotalPages)
            {
                var link = this.UriTemplateSubstitutionParams == null // kbr
                               ? this.uriTemplate.CreateLink("next", new { Page = this.Page + 1 })
                               : this.uriTemplate.CreateLink("next", this.UriTemplateSubstitutionParams, new { Page = this.Page + 1 }); // page overrides UriTemplateSubstitutionParams
                this.Links.Add(link);
            }

            this.Links.Add(new Link("page", this.uriTemplate.Href));
        }
    }
}