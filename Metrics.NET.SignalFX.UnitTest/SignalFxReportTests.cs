using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Metrics.NET.SignalFX.UnitTest.Fakes;
using Metrics.SignalFx;

namespace Metrics.NET.SignalFX.UnitTest
{
    public class SignalFxReportTests
    {
        [Fact]
        public void AddMetrics_TestEqualsGetsEscaped()
        {
            var context = new DefaultMetricsContext();
            var sender = new FakeSignalFxReporter();
            var report = new SignalFxReport(
                             sender, 
                             "FakeApiKey",
                             new Dictionary<string, string> {
                    { "System", "UnitTests" }
                }, 10000);

            var tags = new MetricTags("test\\=string=test\\value");

            var timer = context.Timer("TestTimer", Unit.Calls, SamplingType.FavourRecent, TimeUnit.Microseconds, TimeUnit.Microseconds, tags);
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
                             "FakeApiKey",
                             new Dictionary<string, string> {
                    { "System", "UnitTests" }
                }, 10000);

            var tags = new MetricTags("test=string,noequal");

            var timer = context.Timer("TestTimer", Unit.Calls, SamplingType.FavourRecent, TimeUnit.Microseconds, TimeUnit.Microseconds, tags);
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
