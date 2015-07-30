using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.JScript;
using Nancy.Hal.Configuration;
using Nancy.Responses;
using Nancy.Responses.Negotiation;

namespace Nancy.Hal.Processors
{
    public class HalJsonResponseProcessor : IResponseProcessor
    {
        private const string ContentType = "application/hal+json";
        private readonly IProvideHalTypeConfiguration configuration;
        private readonly ISerializer serializer;

        public HalJsonResponseProcessor(IProvideHalTypeConfiguration configuration, IEnumerable<ISerializer> serializers)
        {
            this.configuration = configuration;
            serializer = serializers.FirstOrDefault(x => x.CanSerialize("application/json")); //any json serializer will do
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
            return new JsonResponse(BuildHypermedia(model, context), serializer)
                       {
                           ContentType = "application/hal+json"
                       };
        }

        private dynamic BuildHypermedia(object model, NancyContext context)
        {
            if (model == null) return null;

            if (model is IEnumerable)
            {
                //how to handle a collection at the root resource level?
                return ((IEnumerable)model).Cast<object>().Select(x => BuildHypermedia(x, context));
            }

            IDictionary<string, object> halModel = model.ToDynamic();
            var globalTypeConfig = configuration.GetTypeConfiguration(model.GetType());
            var localTypeConfig = context.LocalHalConfig().GetTypeConfiguration(model.GetType());

            var typeConfig = new AggregatingHalTypeConfiguration(new List<IHalTypeConfiguration> { globalTypeConfig, localTypeConfig });

            var links = typeConfig.LinksFor(model, context).ToArray();
            if (links.Any())
                halModel["_links"] = links.GroupBy(l => l.Rel).ToDictionary(grp => grp.Key, grp => BuildDynamicLinksOrLink(grp));

            var embeddedResources = typeConfig.Embedded().ToArray();
            if (embeddedResources.Any())
            {
                // Remove original objects from the model (if they exist)
                foreach (var embedded in embeddedResources)
                    halModel.Remove(embedded.OriginalPropertyName);
                halModel["_embedded"] = embeddedResources.ToDictionary(info => info.Rel, info => BuildHypermedia(info.GetEmbeddedResource(model), context));
            }

            var ignoredProperties = typeConfig.Ignored().ToArray();
            if (ignoredProperties.Any())
            {
                //remove ignored properties from the output
                foreach (var ignored in ignoredProperties) halModel.Remove(ignored);
            }
            return halModel;
        }

        private static dynamic BuildDynamicLinksOrLink(IEnumerable<Link> grp)
        {
            return grp.Count()>1 ? grp.Select(l=>BuildDynamicLink(l)) : BuildDynamicLink(grp.First());
        }

        private static dynamic BuildDynamicLink(Link link)
        {
            dynamic dynamicLink = new ExpandoObject();
            dynamicLink.href = link.Href;
            if (link.IsTemplated) dynamicLink.templated = true;
            if (!string.IsNullOrEmpty(link.Title)) dynamicLink.title = link.Title;
            return dynamicLink;
        }

        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings { get { return Enumerable.Empty <Tuple<string, MediaRange>>(); } }
    }
}