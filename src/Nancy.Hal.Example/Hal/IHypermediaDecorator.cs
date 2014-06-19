namespace Nancy.Hal.Example.Hal
{
    using System;

    public interface IHypermediaDecorator
    {
        bool CanDecorate(Type type);

        HypermediaRepresentation Process(dynamic model, NancyContext context);
    }
}