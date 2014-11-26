using com.signalfuse.metrics.protobuf;
using Metrics.MetricData;
using Metrics.Reporters;
using Metrics.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Metrics.SignalFx
{
    class SignalFxReport : BaseReport
    {
        private static readonly char[] EQUAL_SPLIT_CHARS = new char[] { '=' };
        private static readonly string SOURCE_DIMENSION = "source";
        private static readonly string SF_SOURCE = "sf_source";
        private static readonly Regex invalid = new Regex(@"[^a-zA-Z0-9\-_]+", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex slash = new Regex(@"\s*/\s*", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly SignalFxReporter sender;
        private readonly string defaultSource;
        private DataPointUploadMessage.Builder uploadMessage;


        public SignalFxReport(SignalFxReporter sender, string defaultSource)
        {
            this.sender = sender;
            this.defaultSource = defaultSource;
        }

        protected override void StartReport(string contextName, DateTime timestamp)
        {
            this.uploadMessage = DataPointUploadMessage.CreateBuilder();
            base.StartReport(contextName, timestamp);
        }

        protected override void EndReport(string contextName, DateTime timestamp)
        {
            base.EndReport(contextName, timestamp);
            this.sender.send(uploadMessage.Build());
        }

        protected override void ReportGauge(string name, double value, Unit unit, MetricTags tags)
        {
            if (!double.IsNaN(value) && !double.IsInfinity(value))
            {
                AddGauge(Name(name, unit), value, tags);
            }
        }

        protected override void ReportCounter(string name, CounterValue value, Unit unit, MetricTags tags)
        {
            if (value.Items.Length == 0)
            {
                AddCumulativeCounter(Name(name, unit), value.Count, tags);
            }
            else
            {
                AddCumulativeCounter(SubfolderName(name, unit, "Total"), value.Count, tags);
            }

            foreach (var item in value.Items)
            {
                AddCumulativeCounter(SubfolderName(name, unit, item.Item), item.Count, tags);
                AddGauge(SubfolderName(name, unit, item.Item, "Percent"), item.Percent, tags);
            }
        }

        protected override void ReportHealth(HealthStatus status) { }

        protected override void ReportHistogram(string name, HistogramValue value, Unit unit, MetricTags tags)
        {
            AddCumulativeCounter(SubfolderName(name, unit, "Count"), value.Count, tags);
            AddGauge(SubfolderName(name, unit, "Last"), value.LastValue, tags);
            AddGauge(SubfolderName(name, unit, "Min"), value.Min, tags);
            AddGauge(SubfolderName(name, unit, "Mean"), value.Mean, tags);
            AddGauge(SubfolderName(name, unit, "Max"), value.Max, tags);
            AddGauge(SubfolderName(name, unit, "StdDev"), value.StdDev, tags);
            AddGauge(SubfolderName(name, unit, "p75"), value.Median, tags);
            AddGauge(SubfolderName(name, unit, "p95"), value.Percentile75, tags);
            AddGauge(SubfolderName(name, unit, "p95"), value.Percentile95, tags);
            AddGauge(SubfolderName(name, unit, "p98"), value.Percentile98, tags);
            AddGauge(SubfolderName(name, unit, "p99"), value.Percentile99, tags);
            AddGauge(SubfolderName(name, unit, "p999"), value.Percentile999, tags);
        }

        protected override void ReportMeter(string name, MeterValue value, Unit unit, TimeUnit rateUnit, MetricTags tags)
        {
            AddCumulativeCounter(SubfolderName(name, unit, "Total"), value.Count, tags);
            AddGauge(SubfolderName(name, AsRate(unit, rateUnit), "Rate-Mean"), value.MeanRate, tags);
            AddGauge(SubfolderName(name, AsRate(unit, rateUnit), "Rate-1-min"), value.OneMinuteRate, tags);
            AddGauge(SubfolderName(name, AsRate(unit, rateUnit), "Rate-5-min"), value.FiveMinuteRate, tags);
            AddGauge(SubfolderName(name, AsRate(unit, rateUnit), "Rate-15-min"), value.FifteenMinuteRate, tags);

            foreach (var item in value.Items)
            {
                AddGauge(SubfolderName(name, unit, item.Item, "Percent"), item.Percent, tags);
                AddCumulativeCounter(SubfolderName(name, unit, item.Item, "Count"), item.Value.Count, tags);
                AddGauge(SubfolderName(name, AsRate(unit, rateUnit), item.Item, "Rate-Mean"), item.Value.MeanRate, tags);
                AddGauge(SubfolderName(name, AsRate(unit, rateUnit), item.Item, "Rate-1-min"), item.Value.OneMinuteRate, tags);
                AddGauge(SubfolderName(name, AsRate(unit, rateUnit), item.Item, "Rate-5-min"), item.Value.FiveMinuteRate, tags);
                AddGauge(SubfolderName(name, AsRate(unit, rateUnit), item.Item, "Rate-15-min"), item.Value.FifteenMinuteRate, tags);
            }
        }

        protected override void ReportTimer(string name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags)
        {
            AddCumulativeCounter(SubfolderName(name, unit, "Count"), value.Rate.Count, tags);
            AddGauge(SubfolderName(name, unit, "Active_Sessions"), value.ActiveSessions, tags);

            AddGauge(SubfolderName(name, AsRate(unit, rateUnit), "Rate-Mean"), value.Rate.MeanRate, tags);
            AddGauge(SubfolderName(name, AsRate(unit, rateUnit), "Rate-1-min"), value.Rate.OneMinuteRate, tags);
            AddGauge(SubfolderName(name, AsRate(unit, rateUnit), "Rate-5-min"), value.Rate.FiveMinuteRate, tags);
            AddGauge(SubfolderName(name, AsRate(unit, rateUnit), "Rate-15-min"), value.Rate.FifteenMinuteRate, tags);

            AddGauge(SubfolderName(name, durationUnit.Unit(), "Duration-Last"), value.Histogram.LastValue, tags);
            AddGauge(SubfolderName(name, durationUnit.Unit(), "Duration-Min"), value.Histogram.Min, tags);
            AddGauge(SubfolderName(name, durationUnit.Unit(), "Duration-Mean"), value.Histogram.Mean, tags);
            AddGauge(SubfolderName(name, durationUnit.Unit(), "Duration-Max"), value.Histogram.Max, tags);
            AddGauge(SubfolderName(name, durationUnit.Unit(), "Duration-StdDev"), value.Histogram.StdDev, tags);
            AddGauge(SubfolderName(name, durationUnit.Unit(), "Duration-p75"), value.Histogram.Median, tags);
            AddGauge(SubfolderName(name, durationUnit.Unit(), "Duration-p95"), value.Histogram.Percentile75, tags);
            AddGauge(SubfolderName(name, durationUnit.Unit(), "Duration-p95"), value.Histogram.Percentile95, tags);
            AddGauge(SubfolderName(name, durationUnit.Unit(), "Duration-p98"), value.Histogram.Percentile98, tags);
            AddGauge(SubfolderName(name, durationUnit.Unit(), "Duration-p99"), value.Histogram.Percentile99, tags);
            AddGauge(SubfolderName(name, durationUnit.Unit(), "Duration-p999"), value.Histogram.Percentile999, tags);

        }

        protected virtual void AddGauge(string name, double value, MetricTags tags)
        {
            Add(name, value, com.signalfuse.metrics.protobuf.MetricType.GAUGE, tags);
        }

        protected virtual void AddGauge(string name, long value, MetricTags tags)
        {
            Add(name, value, com.signalfuse.metrics.protobuf.MetricType.GAUGE, tags);
        }

        protected virtual void AddCumulativeCounter(string name, double value, MetricTags tags)
        {
            Add(name, value, com.signalfuse.metrics.protobuf.MetricType.CUMULATIVE_COUNTER, tags);
        }

        protected virtual void Add(string name, double value, com.signalfuse.metrics.protobuf.MetricType metricType, MetricTags tags)
        {
            Datum.Builder datum = Datum.CreateBuilder();
            datum.SetDoubleValue(value);
            Add(datum.Build(), name, metricType, tags);
        }

        protected virtual void Add(string name, long value, com.signalfuse.metrics.protobuf.MetricType metricType, MetricTags tags)
        {
            Datum.Builder datum = Datum.CreateBuilder();
            datum.SetIntValue(value);

            Add(datum.Build(), name, metricType, tags);
        }

        protected virtual void Add(Datum value, string name, com.signalfuse.metrics.protobuf.MetricType metricType, MetricTags tags)
        {
            DataPoint.Builder dataPoint = DataPoint.CreateBuilder();
            dataPoint.SetValue(value);
            dataPoint.SetMetric(name);
            dataPoint.SetMetricType(metricType);
            //dataPoint.SetTimestamp(Timestamp.ToUnixTime());
            var sourceName = AddDimensions(dataPoint, tags);
            if (sourceName == null) {
                sourceName = defaultSource;
            }
            AddDimension(dataPoint, SF_SOURCE, sourceName);
            uploadMessage.AddDatapoints(dataPoint);

        }

        protected virtual string AddDimensions(DataPoint.Builder dataPoint, MetricTags tags)
        {

            string source = null;
            foreach (var str in tags.Tags)
            {
                var nameValue = str.Split(EQUAL_SPLIT_CHARS, 2);
                if (nameValue.Length == 2)
                {
                    if (nameValue[0] == SOURCE_DIMENSION)
                    {
                        source = nameValue[0];
                    }
                    AddDimension(dataPoint, nameValue[0], nameValue[1]);
                }
            }
            return source;
        }

        protected virtual void AddDimension(DataPoint.Builder dataPoint, string key, string value)
        {
            Dimension.Builder dimension = Dimension.CreateBuilder();
            dimension.SetKey(key);
            dimension.SetValue(value);
            dataPoint.AddDimensions(dimension);
        }
        protected virtual string AsRate(Unit unit, TimeUnit rateUnit)
        {
            return string.Concat(unit.Name, "-per-", rateUnit.Unit());
        }

        protected virtual string SubfolderName(string cleanName, Unit unit, string itemName, string itemSuffix)
        {
            return Name(string.Concat(cleanName, ".", GraphiteName(itemName), "-", GraphiteName(itemSuffix)), unit);
        }

        protected virtual string SubfolderName(string cleanName, Unit unit, string itemSuffix)
        {
            return Name(string.Concat(cleanName, ".", GraphiteName(itemSuffix)), unit);
        }

        protected virtual string Name(string cleanName, Unit unit, string itemSuffix)
        {
            return Name(string.Concat(cleanName, "-", GraphiteName(itemSuffix)), unit);
        }


        protected virtual string Name(string cleanName, Unit unit)
        {
            return Name(cleanName, unit.Name);
        }

        protected virtual string Name(string cleanName, string unit)
        {
            return string.Concat(cleanName, FormatUnit(unit, cleanName));
        }

        protected virtual string FormatUnit(string unit, string name)
        {
            if (string.IsNullOrEmpty(unit))
            {
                return string.Empty;
            }
            var clean = GraphiteName(unit);

            if (string.IsNullOrEmpty(clean))
            {
                return string.Empty;
            }

            if (name.EndsWith(clean, StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Empty;
            }

            return string.Concat("-", clean);
        }
        protected override string FormatContextName(IEnumerable<string> contextStack, string contextName)
        {
            var parts = contextStack.Concat(new[] { contextName })
                .Select(c => GraphiteName(c));

            return string.Join(".", parts);
        }

        protected override string FormatMetricName<T>(string context, MetricValueSource<T> metric)
        {
            return string.Concat(context, ".", GraphiteName(metric.Name));
        }

        protected virtual string GraphiteName(string name)
        {
            var noSlash = slash.Replace(name, "-per-");
            return invalid.Replace(noSlash, "_").Trim('_');
        }


    }
}
