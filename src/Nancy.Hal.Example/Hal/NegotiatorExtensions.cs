namespace Nancy.Hal.Example.Hal
{
    using Nancy.Responses.Negotiation;

    public static class NegotiatorExtensions
    {
        public static Negotiator WithHalModel<TModel, TResource>(this Negotiator negotiator, TModel model)
        {
            var resource = AutoMapper.Mapper.Map<TModel, TResource>(model);
            return negotiator.WithMediaRangeModel("application/hal+json", resource);
        }

        public static Negotiator WithHalModel(this Negotiator negotiator, IResource resource)
        {
            return negotiator.WithMediaRangeModel("application/hal+json", resource);
        }
    }
}