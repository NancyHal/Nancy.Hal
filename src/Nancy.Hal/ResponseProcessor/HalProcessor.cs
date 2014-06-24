namespace Nancy.Hal.ResponseProcessor
{
    using System;
    using System.Collections.Generic;

    using Nancy.Responses;
    using Nancy.Responses.Negotiation;
    using Nancy.Serialization.JsonNet;

    using Newtonsoft.Json;

    public class HalProcessor : IResponseProcessor
    {
        private JsonSerializer serializer;

        private readonly HalJsonConfiguration configuration;

        public HalProcessor(JsonSerializer serializer, HalJsonConfiguration configuration)
        {
            this.serializer = serializer;
            this.configuration = configuration;
        }

        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            if (requestedMediaRange.Matches("application/hal+json"))
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
            serializer.ContractResolver = new JsonNetHalJsonContactResolver(configuration, context);
            return new JsonResponse(model, new JsonNetSerializer(serializer));
        }

        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings { get; private set; }
    }
}