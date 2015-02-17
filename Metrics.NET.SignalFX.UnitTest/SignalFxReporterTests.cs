using System.Collections.Generic;
using System.Linq;
using System.Threading;
using com.signalfuse.metrics.protobuf;
using Metrics.NET.SignalFX.UnitTest.Fakes;
using Metrics.SignalFx;
using Xunit;

namespace Metrics.NET.SignalFX.UnitTest
{
    public class SignalFxReporterTests
    {
        [Fact]
        public void Send_EnsureMetricsGetReported()
        {
            var context = new DefaultMetricsContext();

            var requestor = new FakeRequestorFactory();
            var sender = new SignalFxReporter("http://fake.signalfuse.com", "ABC123", requestor);
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

            var reportedBytes = requestor.WrittenData;
            Assert.True(reportedBytes.Length > 0);

            var message = DataPointUploadMessage.ParseFrom(reportedBytes);

            var dp = message.DatapointsList.FirstOrDefault(datapoint => datapoint.DimensionsList.Any(dimension => dimension.Key == "test=string"));
            Assert.NotNull(dp);

            var dm = dp.DimensionsList.FirstOrDefault(dimension => dimension.Key == "test=string");
            Assert.Equal("testvalue", dm.Value);
        }

        [Fact]
        public void Send_EnsureApiKeyDoesntTriggerErrorHandler()
        {
            var context = new DefaultMetricsContext();

            var requestor = new GenericFakeRequestorFactory<FakeAccessDeniedRequestor>();
            var sender = new SignalFxReporter("http://fake.signalfuse.com", "ABC123", requestor);
            var report = new SignalFxReport(
                        sender,
                        "FakeApiKey",
                        new Dictionary<string, string> {
                    { "System", "UnitTests" }
                }, 10000);

            int errorCount = 0;
            new MetricsConfig(context).WithErrorHandler((exc, msg) => errorCount++, true);

            var tags = new MetricTags("test\\=string=test\\value");

            var timer = context.Timer("TestTimer", Unit.Calls, SamplingType.FavourRecent, TimeUnit.Microseconds, TimeUnit.Microseconds, tags);
            timer.Record(10053, TimeUnit.Microseconds);

            var source = new CancellationTokenSource();
            report.RunReport(context.DataProvider.CurrentMetricsData, () => new HealthStatus(), source.Token);

            Assert.True(errorCount < 1);
        }

        [Fact]
        public void Send_EnsureWebExceptionTriggersErrorHandler()
        {
            var context = new DefaultMetricsContext();

            var requestor = new GenericFakeRequestorFactory<FakeWebExceptionRequestor>();
            var sender = new SignalFxReporter("http://fake.signalfuse.com", "ABC123", requestor);
            var report = new SignalFxReport(
                        sender,
                        "FakeApiKey",
                        new Dictionary<string, string> {
                    { "System", "UnitTests" }
                }, 10000);

            int errorCount = 0;
            new MetricsConfig(context).WithErrorHandler((exc, msg) => errorCount++, true);

            var tags = new MetricTags("test\\=string=test\\value");

            var timer = context.Timer("TestTimer", Unit.Calls, SamplingType.FavourRecent, TimeUnit.Microseconds, TimeUnit.Microseconds, tags);
            timer.Record(10053, TimeUnit.Microseconds);

            var source = new CancellationTokenSource();
            report.RunReport(context.DataProvider.CurrentMetricsData, () => new HealthStatus(), source.Token);

            Assert.True(errorCount >= 1);
        }
    }
}
