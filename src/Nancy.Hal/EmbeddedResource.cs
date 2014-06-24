namespace Nancy.Hal
{
    using System;
    using System.Reflection;

    public class EmbeddedResource
    {
        public EmbeddedResource(string rel, PropertyInfo propertyInfo, Func<object, object> getter)
        {
            this.Rel = rel;
            this.PropertyInfo = propertyInfo;
            this.Getter = getter;
        }

        public string Rel { get; private set; }

        public PropertyInfo PropertyInfo { get; private set; }

        public Func<object, object> Getter { get; private set; }
    }

    public class EmbeddedResource<T, TEmbedded> : EmbeddedResource
    {
        public EmbeddedResource(string rel, PropertyInfo propertyInfo, Func<T, TEmbedded> getter) : base(rel, propertyInfo, (o) => getter((T)o))
        {
        }
    }
}