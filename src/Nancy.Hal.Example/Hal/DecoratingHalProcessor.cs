namespace Nancy.Hal.Example.Hal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Nancy.Responses.Negotiation;

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
                && this.decorators.Any(x => x.CanDecorate(model.GetType())))
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
            var decorator = this.decorators.First(x => x.CanDecorate(model.GetType()));
            var representation = decorator.Process(model, context);

            return new HalResponse(representation, this.serializer);
        }
    }
}