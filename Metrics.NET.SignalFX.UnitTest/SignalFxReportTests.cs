using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Metrics.Core;
using Xunit;
using Metrics.NET.SignalFx.UnitTest.Fakes;
using Metrics.SignalFx;
using Metrics.SignalFX;

namespace Metrics.NET.SignalFx.UnitTest
{
    public class SignalFxReportTests
    {
        [Fact]
        public void AddMetrics_EnsureAllDimensionsGetSent()
        {
            var context = new TaggedMetricsContext();
            var sender = new FakeSignalFxReporter();
            var report = new SignalFxReport(
                             sender,
                             "",
                             "FakeApiKey",
                             new Dictionary<string, string> {
                    { "System", "UnitTests" },
                }, 10000, new HashSet<MetricDetails> { MetricDetails.count });

            var accountNoRandom = new Random();
            // 
            var accountId = new byte[16];
            var generatedHases = new HashSet<string>();

            while (generatedHases.Count < 50)
            {
                accountNoRandom.NextBytes(accountId);
                var accountIdString = accountId.Aggregate(new StringBuilder(), (builder, b) => builder.AppendFormat("{0:x2}", b)).ToString();

                if (generatedHases.Contains(accountIdString))
                {
                    continue;
                }

                generatedHases.Add(accountIdString);
                var tags = new MetricTags("account=" + accountIdString);
                var counter = context.Counter("TestCounter", Unit.Calls, tags);
                counter.Increment(accountNoRandom.Next());
            }

            var source = new CancellationTokenSource();
            report.RunReport(context.DataProvider.CurrentMetricsData, () => new HealthStatus(), source.Token);

            var dimensions = sender.SelectMany(message =>
                message.datapoints.SelectMany(datapoint => datapoint.dimensions));

            Assert.True(dimensions.Count() > 50);
        }

        [Fact]
        public void AddMetrics_EnsureLimitIsRespected()
        {
            var context = new DefaultMetricsContext();
            var sender = new FakeSignalFxReporter();
            var report = new SignalFxReport(
                             sender,
                             "",
                             "FakeApiKey",
                             new Dictionary<string, string> {
                    { "System", "UnitTests" }
                }, 50, new HashSet<MetricDetails> { MetricDetails.count });

            var accountNoRandom = new Random();
            // 
            var accountId = new byte[16];
            var generatedHases = new HashSet<string>();

            while (generatedHases.Count < 51)
            {
                accountNoRandom.NextBytes(accountId);
                var accountIdString = accountId.Aggregate(new StringBuilder(), (builder, b) => builder.AppendFormat("{0:x2}", b)).ToString();

                if (generatedHases.Contains(accountIdString))
                {
                    continue;
                }

                generatedHases.Add(accountIdString);
                var counter = context.Counter("TestCounter", Unit.Calls, new MetricTags());
                counter.Increment(accountIdString, accountNoRandom.Next());
            }

            var source = new CancellationTokenSource();
            report.RunReport(context.DataProvider.CurrentMetricsData, () => new HealthStatus(), source.Token);

            Assert.True(sender.Count >= 1);
            var message = sender[0];

            Assert.Equal(50, message.datapoints.Count);
        }

        [Fact]
        public void AddMetrics_AllMetricsGetReported()
        {
            var context = new DefaultMetricsContext();
            var sender = new FakeSignalFxReporter();
            var report = new SignalFxReport(
                             sender,
                             "",
                             "FakeApiKey",
                             new Dictionary<string, string> {
                    { "System", "UnitTests" }
                }, 10000, null);

            var tags = new MetricTags("test=value");

            var totalExpectedMetrics = 0;
			var timer = context.Timer("TestTimer", Unit.Calls, SamplingType.ExponentiallyDecaying, TimeUnit.Microseconds, TimeUnit.Microseconds, tags);
            timer.Record(10053, TimeUnit.Microseconds);

            // A single Timer produces 17 metrics
            //   Count
            //   Active_Sessions
            //   Rate-Mean-Calls-per-us
            //   Rate-1-min-Calls-per-us
            //   Rate-5-min-Calls-per-us
            //   Rate-15-min-Calls-per-us
            //   Duration-Last-us
            //   Duration-Min-us
            //   Duration-Mean-us
            //   Duration-Max-us
            //   Duration-StdDev-us
            //   Duration-p75-us
            //   Duration-p95-us
            //   Duration-p95-us
            //   Duration-p98-us
            //   Duration-p99-us
            //   Duration-p999-us
            totalExpectedMetrics += 17;

            context.Gauge("TestGuage", () => 3.3, Unit.KiloBytes, tags);
            // A gauge is a single metric
            totalExpectedMetrics += 1;

            var counter = context.Counter("TestCounter", Unit.KiloBytes, tags);
            counter.Increment("SetA", 2);
            counter.Increment("SetB", 5);

            // We're setting two sub counters within counter
            // plus there is a "Total" counter
            // and for each of the two, there is a total and a percentage
            totalExpectedMetrics += 5;

			var histogram = context.Histogram("TestHistogram", Unit.Events, SamplingType.ExponentiallyDecaying, tags);
            histogram.Update(23, "ABC");
            histogram.Update(14, "DEF");

            // Histogram of events produces 12 metrics:
            //   Count-Events
            //   Last-Events
            //   Min-Events
            //   Mean-Events
            //   Max-Events
            //   StdDev-Events
            //   p75-Events
            //   p95-Events
            //   p95-Events
            //   p98-Events
            //   p99-Events
            //   p999-Events
            totalExpectedMetrics += 12;

            var meter = context.Meter("TestMeter", Unit.MegaBytes, TimeUnit.Seconds, tags);
            meter.Mark("A", 12);
            meter.Mark("B", 190);

            // Meters result in the following 5 metrics for MB
            //   Total-Mb
            //   Rate-Mean-Mb
            //   Rate-1-min-Mb
            //   Rate-5-min-Mb
            //   Rate-15-min-Mb
            //
            //   And then for each item that is marked, there's a set of these 6
            //   Percent-Mb
            //   Count-Mb
            //   Rate-Mean-Mb-per-s
            //   Rate-1-min-Mb-per-s
            //   Rate-5-min-Mb-per-s
            //   Rate-15-min-Mb-per-s
            totalExpectedMetrics += 17;

            // so our total metrics now is 52
            var source = new CancellationTokenSource();
            report.RunReport(context.DataProvider.CurrentMetricsData, () => new HealthStatus(), source.Token);

            Assert.Equal(1, sender.Count);
            var message = sender[0];

            Assert.Equal(totalExpectedMetrics, message.datapoints.Count);
        }

        [Fact]
        public void AddMetrics_TestEqualsGetsEscaped()
        {
            var context = new DefaultMetricsContext();
            var sender = new FakeSignalFxReporter();
            var report = new SignalFxReport(
                             sender,
                             "",
                             "FakeApiKey",
                             new Dictionary<string, string> {
                    { "System", "UnitTests" }
                }, 10000, new HashSet<MetricDetails> { MetricDetails.count });

            var tags = new MetricTags("test\\=string=test\\value");

			var timer = context.Timer("TestTimer", Unit.Calls, SamplingType.ExponentiallyDecaying, TimeUnit.Microseconds, TimeUnit.Microseconds, tags);
            timer.Record(10053, TimeUnit.Microseconds);

            var source = new CancellationTokenSource();
            report.RunReport(context.DataProvider.CurrentMetricsData, () => new HealthStatus(), source.Token);

            Assert.Equal(1, sender.Count);
            var message = sender[0];

            var dp = message.datapoints.FirstOrDefault(datapoint => datapoint.dimensions.Any(dimension => dimension.key == "test=string"));
            Assert.NotNull(dp);

            var dm = dp.dimensions.FirstOrDefault(dimension => dimension.key == "test=string");
            Assert.Equal("testvalue", dm.value);
        }

        [Fact]
        public void AddMetrics_TestNoEqualTagNotAddedAsDimension()
        {
            var context = new DefaultMetricsContext();
            var sender = new FakeSignalFxReporter();
            var report = new SignalFxReport(
                             sender,
                             "",
                             "FakeApiKey",
                             new Dictionary<string, string> {
                    { "System", "UnitTests" }
                }, 10000, new HashSet<MetricDetails> { MetricDetails.count });

            var tags = new MetricTags("test=string,noequal");

			var timer = context.Timer("TestTimer", Unit.Calls, SamplingType.ExponentiallyDecaying, TimeUnit.Microseconds, TimeUnit.Microseconds, tags);
            timer.Record(10053, TimeUnit.Microseconds);

            var source = new CancellationTokenSource();
            report.RunReport(context.DataProvider.CurrentMetricsData, () => new HealthStatus(), source.Token);

            Assert.Equal(1, sender.Count);
            var message = sender[0];

            var dp = message.datapoints.FirstOrDefault(datapoint => datapoint.dimensions.Any(dimension => dimension.key == "test"));
            Assert.NotNull(dp);

            var dp1 = message.datapoints.FirstOrDefault(datapoint => datapoint.dimensions.Any(dimension => dimension.key == "noequal"));
            Assert.Null(dp1);

            var dm = dp.dimensions.FirstOrDefault(dimension => dimension.key == "test");
            Assert.Equal("string", dm.value);

            dm = dp.dimensions.FirstOrDefault(dimension => dimension.key == "noequal");
            Assert.Null(dm);
        }
    }
}
