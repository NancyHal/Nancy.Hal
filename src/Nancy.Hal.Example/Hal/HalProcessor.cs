namespace Nancy.Hal.Example.Hal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.Hal.Example.Model;
    using Nancy.Hal.Example.Model.Users.ViewModels;
    using Nancy.Responses.Negotiation;

    public class HalProcessor : IResponseProcessor
    {
        private readonly ISerializer serializer;

        public HalProcessor(IEnumerable<ISerializer> serializers)
        {
            this.serializer = serializers.FirstOrDefault(x => x.CanSerialize("application/hal+json"));
        }

        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings { get { return new Tuple<string, MediaRange>[0]; } }

        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            if (requestedMediaRange.Matches("application/hal+json") && model is IResource)
            {
                return new ProcessorMatch
                {
                    ModelResult = MatchResult.ExactMatch,
                    RequestedContentTypeResult = MatchResult.ExactMatch
                };
            }

            return new ProcessorMatch
            {
                ModelResult = MatchResult.DontCare,
                RequestedContentTypeResult = MatchResult.NoMatch
            };
        }

        public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            var resource = model as IResource;
            if (resource != null)
            {
                foreach (var link in resource.Links)
                {
                    var uri = new UriBuilder(link.Href);
                    uri.Host = context.Request.Url.HostName;
                    uri.Port = context.Request.Url.Port ?? (context.Request.Url.Scheme == "https" ? 443 : 80);
                    link.Href = uri.ToString();
                }
            }
            return new HalResponse(model, this.serializer);
        }
    }

    public class DecoratingHalProcessor : IResponseProcessor
    {
        private readonly ISerializer serializer;

        private IList<IHypermediaDecorator> decorators;

        public DecoratingHalProcessor(IEnumerable<ISerializer> serializers, IEnumerable<IHypermediaDecorator> decorators)
        {
            this.decorators = decorators.ToList();
            this.serializer = serializers.FirstOrDefault(x => x.CanSerialize("application/hal+json"));
        }

        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings { get { return new Tuple<string, MediaRange>[0]; } }

        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            if (requestedMediaRange.Matches("application/hal+json")
                && !(model is IResource)
                && decorators.Any(x => x.CanDecorate(model.GetType())))
            {
                return new ProcessorMatch
                {
                    ModelResult = MatchResult.ExactMatch,
                    RequestedContentTypeResult = MatchResult.ExactMatch
                };
            }

            return new ProcessorMatch
            {
                ModelResult = MatchResult.DontCare,
                RequestedContentTypeResult = MatchResult.NoMatch
            };
        }

        public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            var decorator = decorators.First(x => x.CanDecorate(model.GetType()));
            var representation = decorator.Process(model, context);

            return new HalResponse(representation, this.serializer);
        }
    }

    public interface IHypermediaDecorator
    {
        bool CanDecorate(Type type);

        HypermediaRepresentation Process(dynamic model, NancyContext context);
    }

    public class HypermediaRepresentation
    {
        public HypermediaRepresentation()
        {
            Links = new List<Link>();
        }

        public object Model { get; set; }

        public Link Self { get; set; }

        public IList<Link> Links { get; set; } 
    }

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

    public class HypermediaDecoratorRegistration : IRegistrations
    {
        public IEnumerable<TypeRegistration> TypeRegistrations { get; private set; }

        public IEnumerable<CollectionTypeRegistration> CollectionTypeRegistrations { get
        {
            return new[]
                       {
                           new CollectionTypeRegistration(
                               typeof(IHypermediaDecorator),
                               AppDomainAssemblyTypeScanner.TypesOf<IHypermediaDecorator>()),
                       };
        } }

        public IEnumerable<InstanceRegistration> InstanceRegistrations { get; private set; }
    }
}