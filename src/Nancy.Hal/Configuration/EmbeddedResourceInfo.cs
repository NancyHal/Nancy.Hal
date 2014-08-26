namespace Nancy.Hal.Configuration
{
    using System;
    using System.Reflection;

    public interface IEmbeddedResourceInfo
    {
        string Rel { get; }
        string OriginalPropertyName { get; }
        object GetEmbeddedResource(object model);
    }

    public class EmbeddedResourceInfo<TModel> : IEmbeddedResourceInfo
    {
        private readonly Func<TModel, dynamic> getter;
        private readonly PropertyInfo propertyInfo;

        internal EmbeddedResourceInfo(string rel, PropertyInfo propertyInfo, Func<TModel, dynamic> getter)
        {
            Rel = rel;
            this.propertyInfo = propertyInfo;
            this.getter = getter;
        }

        public string Rel { get; private set; }

        public string OriginalPropertyName { get { return propertyInfo.Name; } }

        public object GetEmbeddedResource(object model)
        {
            return getter((TModel)model);
        }
    }
}