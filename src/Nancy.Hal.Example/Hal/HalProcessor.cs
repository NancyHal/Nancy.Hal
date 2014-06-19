namespace Nancy.Hal.Example.Hal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Nancy;
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
}