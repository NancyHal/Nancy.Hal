using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nancy.Hal.Configuration
{
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Defines the string-key to use to store the local HalConfiguration in the Items-dictionary of the NancyContext.
        /// </summary>
        public static string NancyContextKey { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Request a HalTypeConfiguration instance from the local HalConfiguration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static HalTypeConfiguration<T> LocalHalConfigFor<T>(this NancyContext context)
        {
            context.EnsureHalConfiguration();

            return ((HalConfiguration) context.Items[NancyContextKey]).For<T>();
        }

        /// <summary>
        /// Retrieve the local HalConfiguration from the NancyContext
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IProvideHalTypeConfiguration LocalHalConfig(this NancyContext context)
        {
            context.EnsureHalConfiguration();

            return (IProvideHalTypeConfiguration) context.Items[NancyContextKey];
        }

        /// <summary>
        /// Internal use.
        /// Ensures that the current NancyContext stores a HalConfiguration instance
        /// </summary>
        /// <param name="context"></param>
        private static void EnsureHalConfiguration(this NancyContext context)
        {
            bool contextStoresHalConfig = context.Items.ContainsKey(NancyContextKey);

            if (!contextStoresHalConfig)
            {
                context.Items[NancyContextKey] = new HalConfiguration();
            }
        }
    }
}
