namespace Nancy.Hal.Example.Hal
{
    using System.Collections.Generic;

    public class HypermediaRepresentation
    {
        public HypermediaRepresentation()
        {
            this.Links = new List<Link>();
        }

        public object Model { get; set; }

        public Link Self { get; set; }

        public IList<Link> Links { get; set; } 
    }
}