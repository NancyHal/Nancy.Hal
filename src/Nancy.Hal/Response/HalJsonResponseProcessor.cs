namespace Nancy.Hal.ResponseProcessor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Nancy.Hal.Configuration;
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

        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings { get { return Enumerable.Empty<Tuple<string, MediaRange>>(); } }

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
            if (model == null) return new HalJsonResponse(model, serializer);

            if (!(model is IEnumerable))
            {
                dynamic halRepresentation = ((object)model).ToDynamicWithCamelCaseProperties();
                BuildHypermedia(halRepresentation, model, context);
                return new HalJsonResponse(halRepresentation, serializer);
            }
            else
            {
                throw new NotSupportedException(
                    model.GetType()
                    + "is not supported by the hal+json media type. Collections should be modelled as an embedded resource.");
            }
        }

        private void BuildHypermedia(dynamic halModel, object model, NancyContext context, bool isRoot = true)
        {
            HalJsonTypeConfiguration typeConfig;
            configuration.TryGetTypeConfiguration(model.GetType(), out typeConfig);

            if (typeConfig == null) return;

            var dict = halModel as IDictionary<string, object>;
            foreach (var key in dict.Keys.ToList())
            {
                if (dict[key] == null) dict.Remove(key);
            }

            if (typeConfig.Links.Any())
            {
                var links = typeConfig.Links.Select(x => x.Invoke(model, context)).Where(x => x != null);

                halModel._links = links.ToDictionary(
                    link => link.Rel,
                    link => link.ToHalLink());

                if (isRoot && configuration.Curies.Any())
                {
                    halModel._links["curies"] = configuration.Curies.Select(
                        curie =>
                            {
                                dynamic dynamicCurie = curie.Value.ToHalLink();
                                dynamicCurie.name = curie.Key;
                                return dynamicCurie;
                            });
                }
            }

            if (typeConfig.Embedded.Any())
            {
                var embeddedResources = new Dictionary<string, object>(); //typeConfig.Embedded.Values.ToDictionary(x => x.Rel, x => x.Getter.Invoke(model));
                foreach (var embedded in typeConfig.Embedded.Values)
                {
                    var embeddedModel = embedded.Getter.Invoke(model);
                    if (embeddedModel != null)
                    {
                        ((IDictionary<string,Object>)halModel).Remove(embedded.PropertyInfo.Name.ToCamelCaseString());

                        if (!(embeddedModel is IEnumerable))
                        {
                            IDictionary<string, object> embeddedHalRepresentation = embeddedModel.ToDynamicWithCamelCaseProperties();
                            BuildHypermedia(embeddedHalRepresentation, embeddedModel, context, isRoot: false);
                            embeddedResources[embedded.Rel] = embeddedHalRepresentation;
                        }
                        else
                        {
                            var embeddedCollection = (IEnumerable)embeddedModel;

                            embeddedResources[embedded.Rel] = embeddedCollection.Cast<object>().Select(
                                x =>
                                {
                                    IDictionary<string, object> embeddedHalRepresentation = x.ToDynamicWithCamelCaseProperties();
                                    BuildHypermedia(embeddedHalRepresentation, x, context, isRoot: false);
                                    return embeddedHalRepresentation;
                                });
                        }
                    }
                }

                halModel._embedded = embeddedResources;
            }
        }
    }
}