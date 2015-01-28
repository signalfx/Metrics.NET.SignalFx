using Metrics.Utils;

namespace Metrics.Core
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
