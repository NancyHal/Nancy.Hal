namespace Nancy.Hal.Configuration
{
    using System;

    public interface IEmbeddedResourceInfo
    {
        string Rel { get; }
        string OriginalPropertyName { get; }
        object GetEmbeddedResource(object model);
    }

    public class EmbeddedResourceInfo<TModel> : IEmbeddedResourceInfo
    {
        private readonly Func<TModel, dynamic> getter;

        internal EmbeddedResourceInfo(string rel, string propertyName, Func<TModel, dynamic> getter)
        {
            Rel = rel;
            OriginalPropertyName = propertyName;
            this.getter = getter;
        }

        public string Rel { get; private set; }

        public string OriginalPropertyName { get; private set; }

        public object GetEmbeddedResource(object model)
        {
            return getter((TModel)model);
        }
    }
}