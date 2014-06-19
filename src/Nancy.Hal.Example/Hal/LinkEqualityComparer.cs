namespace Nancy.Hal.Example.Hal
{
    using System;
    using System.Collections.Generic;

    internal class LinkEqualityComparer : IEqualityComparer<Link>
    {
        public bool Equals(Link l1, Link l2)
        {
            return string.Compare(l1.Href, l2.Href, StringComparison.OrdinalIgnoreCase) == 0 &&
                   string.Compare(l1.Rel, l2.Rel, StringComparison.OrdinalIgnoreCase) == 0;
        }


        public int GetHashCode(Link lnk)
        {
            var str = (string.IsNullOrEmpty(lnk.Rel) ? "norel" : lnk.Rel) + "~" + (string.IsNullOrEmpty(lnk.Href) ? "nohref" : lnk.Href);
            var h = str.GetHashCode();
            return h;
        }
    }
}