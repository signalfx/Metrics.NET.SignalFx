using System.Text;
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
    internal class SignalFxReport : BaseReport
    {
        private static readonly char[] EQUAL_SPLIT_CHARS = new char[] { '=' };
        private static readonly string METRIC_DIMENSION = "metric";
        private static readonly string SOURCE_DIMENSION = "source";
        private static readonly string SF_SOURCE = "sf_source";
        private static readonly HashSet<string> IGNORE_DIMENSIONS = new HashSet<string>();
        static SignalFxReport()
        {
            IGNORE_DIMENSIONS.Add(SOURCE_DIMENSION);
            IGNORE_DIMENSIONS.Add(METRIC_DIMENSION);
        }

        private static readonly Regex invalid = new Regex(@"[^a-zA-Z0-9\-_]+", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex slash = new Regex(@"\s*/\s*", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly ISignalFxReporter sender;
        private readonly String defaultSource;
        private readonly IDictionary<string, string> defaultDimensions;
        private readonly int maxDatapointsPerMessage;
        private DataPointUploadMessage.Builder uploadMessage;
        private int datapointsAdded = 0;


        public SignalFxReport(ISignalFxReporter sender, string defaultSource, IDictionary<string, string> defaultDimensions, int maxDatapointsPerMessage)
        {
            this.sender = sender;
            this.defaultSource = defaultSource;
            this.defaultDimensions = defaultDimensions;
            this.maxDatapointsPerMessage = maxDatapointsPerMessage;
        }

        protected override void StartReport(string contextName)
        {
            this.uploadMessage = DataPointUploadMessage.CreateBuilder();
            base.StartReport(contextName);
        }

        protected override void EndReport(string contextName)
        {
            base.EndReport(contextName);
            this.sender.Send(uploadMessage.Build());
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
            IDictionary<string, string> dimensions = ParseTagsToDimensions(tags);
            DataPoint.Builder dataPoint = DataPoint.CreateBuilder();
            dataPoint.SetValue(value);
            string metricName = dimensions.ContainsKey(METRIC_DIMENSION) ? dimensions[METRIC_DIMENSION] : name;
            string sourceName = dimensions.ContainsKey(SOURCE_DIMENSION) ? dimensions[SOURCE_DIMENSION] : defaultSource;
            dataPoint.SetMetric(metricName);
            dataPoint.SetMetricType(metricType);
            dataPoint.SetSource(sourceName);
            AddDimension(dataPoint, SF_SOURCE, sourceName);

            AddDimensions(dataPoint, defaultDimensions);
            AddDimensions(dataPoint, dimensions);

            uploadMessage.AddDatapoints(dataPoint);

            if (datapointsAdded++ >= maxDatapointsPerMessage)
            {
                this.sender.Send(uploadMessage.Build());
                datapointsAdded = 0;
                this.uploadMessage = DataPointUploadMessage.CreateBuilder();
            }
        }

        protected virtual void AddDimensions(DataPoint.Builder dataPoint, IDictionary<string, string> dimensions)
        {
            foreach (KeyValuePair<string, string> entry in dimensions)
            {
                if (!IGNORE_DIMENSIONS.Contains(entry.Key))
                {
                    AddDimension(dataPoint, entry.Key, entry.Value);
                }
            }
        }

        protected virtual IDictionary<string, string> ParseTagsToDimensions(MetricTags tags)
        {
            IDictionary<string, string> retVal = new Dictionary<string, string>();
            foreach (var str in tags.Tags)
            {
                var nameValue = ParseTag(str);
                retVal[nameValue.Item1] = nameValue.Item2;
            }
            return retVal;
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
                .Select(c => GraphiteName(c)).Skip(1);

            return string.Join(".", parts);
        }

        protected override string FormatMetricName<T>(string context, MetricValueSource<T> metric)
        {
            if (string.IsNullOrEmpty(context))
            {
                return GraphiteName(metric.Name);
            }
            return string.Concat(context, ".", GraphiteName(metric.Name));
        }

        protected virtual string GraphiteName(string name)
        {
            var noSlash = slash.Replace(name, "-per-");
            return invalid.Replace(noSlash, "_").Trim('_');
        }

        private Tuple<string, string> ParseTag(string tag)
        {
            StringBuilder operand = new StringBuilder();

            string left;
            bool escape = false;

            var i = 0;
            for (; i < tag.Length; ++i)
            {
                var chr = tag[i];
                if (!escape)
                {
                    if (chr == '\\')
                    {
                        escape = true;
                    }
                    else
                    {
                        if (chr == '=')
                        {
                            ++i;
                            break;
                        }
                        operand.Append(chr);
                    }
                }
                else
                {
                    escape = false;
                    operand.Append(chr);
                }
            }

            escape = false;
            left = operand.ToString();
            operand.Clear();

            for (; i < tag.Length; ++i)
            {
                var chr = tag[i];
                if (!escape)
                {
                    if (chr == '\\')
                    {
                        escape = true;
                    }
                    else
                    {
                        operand.Append(chr);
                    }
                }
                else
                {
                    operand.Append(chr);
                }
            }
            return new Tuple<string, string>(left, operand.ToString());
        }
    }
}
