using Metrics.Core;
using Metrics.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.SignalFX
{
    public class TaggedMetricsContext : BaseMetricsContext
    {
        public TaggedMetricsContext()
            : this(string.Empty) { }

        public TaggedMetricsContext(string context)
            : base(context, new TaggedMetricsRegistry(), new DefaultMetricsBuilder(), () => Clock.Default.UTCDateTime)
        { }

        protected override MetricsContext CreateChildContextInstance(string contextName)
        {
            return new TaggedMetricsContext(contextName);
        }
    }
}
