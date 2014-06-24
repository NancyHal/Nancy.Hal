namespace Nancy.Hal.ResponseProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

    using Nancy.Hal.Configuration;
    using Nancy.Responses;
    using Nancy.Responses.Negotiation;

    public class HalJsonResponseProcessor : IResponseProcessor
    {
        private const string ContentType = "application/hal+json";
        private readonly HalJsonConfiguration configuration;
        private readonly ISerializer serializer;

        public HalJsonResponseProcessor(HalJsonConfiguration configuration, IEnumerable<ISerializer> serializers)
        {
            this.configuration = configuration;
            this.serializer = serializers.FirstOrDefault(x => x.CanSerialize("application/json")); //any json serializer will do
        }

        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            if (requestedMediaRange.Matches(ContentType))
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
            IDictionary<string, object> halRepresentation = ((object)model).ToDynamic();

            BuildHypermedia(halRepresentation, model, context);

            return new JsonResponse(halRepresentation, serializer);
        }

        private void BuildHypermedia(IDictionary<string, object> halModel, object model, NancyContext context)
        {
            HalJsonTypeConfiguration typeConfig;
            configuration.TryGetTypeConfiguration(model.GetType(), out typeConfig);

            if (typeConfig.Links.Any())
            {
                var links = typeConfig.Links.Select(x => x.Invoke(model, context)).Where(x => x != null);

                halModel["_links"] = links.ToDictionary(
                    link => link.Rel,
                    link =>
                    {
                        dynamic dynamicLink = new ExpandoObject();

                        dynamicLink.href = link.Href;

                        if (link.IsTemplated) dynamicLink.templated = true;

                        if (!string.IsNullOrEmpty(link.Title)) dynamicLink.title = link.Title;

                        return dynamicLink;
                    });
            }

            if (typeConfig.Embedded.Any())
            {
                var embeddedResources = new Dictionary<string, object>(); //typeConfig.Embedded.Values.ToDictionary(x => x.Rel, x => x.Getter.Invoke(model));
                foreach (var embedded in typeConfig.Embedded.Values)
                {
                    var embeddedModel = embedded.Getter.Invoke(model);
                    halModel.Remove(embedded.PropertyInfo.Name);

                    IDictionary<string, object> embeddedHalRepresentation = embeddedModel.ToDynamic();
                    BuildHypermedia(embeddedHalRepresentation, embeddedModel, context);
                    embeddedResources[embedded.Rel] = embeddedHalRepresentation;
                }

                halModel["_embedded"] = embeddedResources;
            }
        }

        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings { get { return Enumerable.Empty <Tuple<string, MediaRange>>(); } }
    }
}