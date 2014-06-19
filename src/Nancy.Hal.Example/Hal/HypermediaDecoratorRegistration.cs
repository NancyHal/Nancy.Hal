namespace Nancy.Hal.Example.Hal
{
    using System.Collections.Generic;

    using Nancy.Bootstrapper;

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