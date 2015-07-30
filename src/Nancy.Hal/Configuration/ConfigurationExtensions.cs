using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nancy.Hal.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IHalTypeConfiguration Merge(this IHalTypeConfiguration self, IHalTypeConfiguration other)
        {
            return new AggregatingHalTypeConfiguration(new List<IHalTypeConfiguration>{ self, other } );
        }
    }
}
