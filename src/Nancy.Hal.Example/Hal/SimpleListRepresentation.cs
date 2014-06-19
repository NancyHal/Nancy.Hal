namespace Nancy.Hal.Example.Hal
{
    using System.Collections.Generic;

    public abstract class SimpleListRepresentation<TResource> : Representation where TResource : IResource
    {
        protected SimpleListRepresentation()
        {
            this.ResourceList = new List<TResource>();
        }

        protected SimpleListRepresentation(IList<TResource> list)
        {
            this.ResourceList = list;
        }

        public IList<TResource> ResourceList { get; set; }
    }
}