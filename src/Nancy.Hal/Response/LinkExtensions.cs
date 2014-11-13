namespace Nancy.Hal.ResponseProcessor
{
    using System.Dynamic;

    public static class LinkExtensions
    {
        public static dynamic ToHalLink(this Link link)
        {
            dynamic dynamicLink = new ExpandoObject();

            dynamicLink.href = link.Href;

            if (link.IsTemplated) dynamicLink.templated = true;

            if (!string.IsNullOrEmpty(link.Title)) dynamicLink.title = link.Title;

            return dynamicLink;
        } 
    }
}