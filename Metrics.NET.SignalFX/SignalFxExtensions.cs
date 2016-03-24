using Metrics.Reporters;
using Metrics.Reports;
using Metrics.SignalFx;
using System;

namespace Metrics
{
    public static class SignalFxExtensions
    {

        public static SignalFxReporterBuilder WithSignalFx(this MetricsReports reports, string apiToken, TimeSpan interval)
        {
            return new SignalFxReporterBuilder(reports, apiToken, interval);
        }


        public static MetricsReports WithSignalFxFromAppConfig(this MetricsReports reports)
        {
            return SignalFxReporterBuilder.FromAppConfig(reports).Build();
        }

    }
}
