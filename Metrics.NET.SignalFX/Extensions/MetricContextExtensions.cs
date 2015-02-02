

using System.Linq;
using Metrics.MetricData;

namespace Metrics.SignalFx
{
    /// <summary>
    /// Extension methods for MetricContexts to support SignalFx
    /// </summary>
    public static class MetricContextExtensions
    {
        /// <summary>
        /// Merge another context into this one
        /// </summary>
        /// <param name="context">the context to merge into</param>
        /// <param name="otherContext">the context to merge</param>
        public static void MergeContext(this MetricsContext context, MetricsContext otherContext)
        {
            var allMetrics = otherContext.DataProvider.CurrentMetricsData;
            foreach (var counter in allMetrics.Counters)
            {
                context.RealCounter(counter).Merge(otherContext.RealCounter(counter));
            }

            foreach (var timer in allMetrics.Timers)
            {
                context.RealTimer(timer).Merge(otherContext.RealTimer(timer));
            }

            foreach (var meter in allMetrics.Meters)
            {
                context.RealMeter(meter).Merge(otherContext.RealMeter(meter));
            }

            foreach (var histogram in allMetrics.Histograms)
            {
                context.RealHistogram(histogram).Merge(otherContext.RealHistogram(histogram));
            }

            var contextGauges = context.DataProvider.CurrentMetricsData.Gauges.ToDictionary(gvs => gvs.Name);
            foreach (var gauge in allMetrics.Gauges)
            {
                GaugeValueSource existingGauge;
                if (contextGauges.TryGetValue(gauge.Name, out existingGauge))
                {
                    // the gauge already exists - so we'll just merge it
                    existingGauge.ValueProvider.Merge(gauge.ValueProvider);
                }
                else
                {
                    var vp = gauge.ValueProvider;
                    context.Gauge(gauge.Name, () => vp.Value, gauge.Unit, gauge.Tags);
                }
            }
        }

        private static Timer RealTimer(this MetricsContext otherContext, TimerValueSource timer)
        {
            return otherContext.Timer(
                timer.Name,
                timer.Unit,
                SamplingType.FavourRecent,
                timer.RateUnit,
                timer.DurationUnit,
                timer.Tags
                );
        }

        private static Histogram RealHistogram(this MetricsContext otherContext, HistogramValueSource histogram)
        {
            return otherContext.Histogram(
                histogram.Name,
                histogram.Unit,
                SamplingType.FavourRecent,
                histogram.Tags
                );
        }

        private static Counter RealCounter(this MetricsContext otherContext, CounterValueSource counter)
        {
            return otherContext.Counter(counter.Name, counter.Unit, counter.Tags);
        }

        private static Meter RealMeter(this MetricsContext otherContext, MeterValueSource meter)
        {
            return otherContext.Meter(meter.Name, meter.Unit, meter.RateUnit, meter.Tags);
        }
    }
}
