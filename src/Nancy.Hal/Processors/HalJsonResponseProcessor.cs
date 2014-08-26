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
            return new JsonResponse(BuildHypermedia(model, context), serializer);
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
            var typeConfig = configuration.GetTypeConfiguration(model.GetType());
            if (typeConfig == null) return halModel;

            var links = typeConfig.LinksFor(model, context).ToArray();
            if (links.Any())
                halModel["_links"] = links.ToDictionary(link => link.Rel, link => BuildDynamicLink(link));

            if (typeConfig.Embedded.Any())
            {
                IDictionary<string, object> embeddedModel = new Dictionary<string, object>();
                foreach (var embedded in typeConfig.Embedded.Values)
                {
                    var embeddedResource = embedded.GetEmbeddedResource(model);
                    if (embeddedResource != null)
                    {
                        halModel.Remove(embedded.OriginalPropertyName);
                        embeddedModel[embedded.Rel] = BuildHypermedia(embeddedResource, context);
                    }
                }
                halModel["_embedded"] = embeddedModel;
            }
            return halModel;
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