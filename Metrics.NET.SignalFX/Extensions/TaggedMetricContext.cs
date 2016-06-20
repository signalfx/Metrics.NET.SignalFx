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

        /// <summary>
        /// Creates a counter that will report the difference in counts between reporting periods instead of the total count. This
        /// is useful if you need a distributed counter.
        /// </summary>
        /// <see cref="MetricContext.Counter"/>
        public Counter IncrementalCounter(string name, Unit unit, MetricTags tags = default(MetricTags))
        {
            return this.Advanced.Counter<IncrementalCounter>(Metrics.SignalFX.Extensions.IncrementalCounter.INC_COUNTER_PREFIX + name,
                unit, () => new IncrementalCounter(), tags);
        }

        /// <summary>
        /// Creates a timer that will only report iff at least one sample has been added since the last time a report was sent.
        /// </summary>
        /// <see cref="MetricsContext.Counter"/>
        public Counter ReportOnUpdateCounter(string name, Unit unit, MetricTags tags = default(MetricTags))
        {
            return Counter(TaggedMetricsRegistry.REPORT_ON_UPDATE_PREFIX + name, unit, tags);
        }

        /// <summary>
        /// Creates a timer that will only report iff at least one sample has been added since the last time a report was sent.
        /// </summary>
        /// <see cref="MetricsContext.Timer"/>
        public Timer ReportOnUpdateTimer(string name, Unit unit,
            SamplingType samplingType = SamplingType.FavourRecent,
            TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds,
            MetricTags tags = default(MetricTags))
        {
            return Timer(TaggedMetricsRegistry.REPORT_ON_UPDATE_PREFIX + name, unit, samplingType, rateUnit, durationUnit, tags);
        }

        /// <summary>
        /// Creates a timer that will only report an incremental counter for the rate and mean.
        /// </summary>
        /// <see cref="MetricsContext.Timer"/>
        public Timer IncrementalTimer(string name, Unit unit,
            SamplingType samplingType = SamplingType.FavourRecent,
            TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds,
            MetricTags tags = default(MetricTags))
        {
            return Timer(TaggedMetricsRegistry.INCREMENTAL_PREFIX + name, unit, samplingType, rateUnit, durationUnit, tags);
        }

        /// <summary>
        /// Creates a meter that will only report iff at least one sample has been added since the last time a report was sent.
        /// </summary>
        /// <see cref="MetricsContext.Meter"/>
        public Meter ReportOnUpdateMeter(string name, Unit unit, TimeUnit rateUnit = TimeUnit.Seconds, MetricTags tags = default(MetricTags))
        {
            return Meter(TaggedMetricsRegistry.REPORT_ON_UPDATE_PREFIX + name, unit, rateUnit, tags);
        }

        /// <summary>
        /// Creates a histogram that will only report iff at least one sample has been added since the last time a report was sent.
        /// </summary>
        /// <see cref="MetricsContext.Histogram"/>
        public Histogram ReportOnUpdateHistogram(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent, MetricTags tags = default(MetricTags))
        {
            return Histogram(TaggedMetricsRegistry.REPORT_ON_UPDATE_PREFIX + name, unit, samplingType, tags);
        }

        protected override MetricsContext CreateChildContextInstance(string contextName)
        {
            return new TaggedMetricsContext(contextName);
        }
    }
}
