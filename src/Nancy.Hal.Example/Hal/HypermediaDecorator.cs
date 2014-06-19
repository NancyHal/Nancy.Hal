namespace Nancy.Hal.Example.Hal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class HypermediaDecorator<T> : IHypermediaDecorator
    {
        public bool CanDecorate(Type type)
        {
            return typeof(T) == type;
        }

        public HypermediaRepresentation Process(dynamic model, NancyContext context)
        {
            return new HypermediaRepresentation() { Model = model, Links = this.BuildHypermedia((T)model, context).ToList() };
        }

        protected abstract IEnumerable<Link> BuildHypermedia(T model, NancyContext context);
    }
}