using Metrics.SignalFX.Extensions;
using Metrics.Utils;

namespace Metrics.Core
{
    public class TaggedMetricsContext : BaseMetricsContext, MetricsContext
    {
        public TaggedMetricsContext()
            : this(string.Empty) { }

        public TaggedMetricsContext(string context)
            : base(context, new TaggedMetricsRegistry(), new DefaultMetricsBuilder(), () => Clock.Default.UTCDateTime)
        { }

        public Counter IncrementalCounter(string name, Unit unit, MetricTags tags = default(MetricTags))
        {
            return this.Advanced.Counter<IncrementalCounter>(Metrics.SignalFX.Extensions.IncrementalCounter.INC_COUNTER_PREFIX + name, 
                unit, () => new IncrementalCounter(), tags);
        }

        protected override MetricsContext CreateChildContextInstance(string contextName)
        {
            return new TaggedMetricsContext(contextName);
        }
    }
}
