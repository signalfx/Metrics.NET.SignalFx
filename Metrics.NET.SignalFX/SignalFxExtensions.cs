using Metrics.Reporters;
using Metrics.Reports;
using Metrics.SignalFx;
using System;

namespace Metrics
{
    public static class SignalFxExtensions
    {

        public static MetricsReports WithSignalFx(this MetricsReports reports, string apiToken, TimeSpan interval)
        {
            return FromSignalFxBuilder(reports, new SignalFxReporterBuilder(apiToken, interval));
        }

        public static MetricsReports FromSignalFxBuilder(this MetricsReports reports, SignalFxReporterBuilder builder)
        {
            Tuple<MetricsReport, TimeSpan> reporterAndInterval = builder.Build();
            if (reporterAndInterval == null)
            {
                return reports;
            }
            return reports.WithReport(reporterAndInterval.Item1, reporterAndInterval.Item2);
        }

        public static MetricsReports WithSignalFxFromAppConfig(this MetricsReports reports)
        {
            return FromSignalFxBuilder(reports, SignalFxReporterBuilder.FromAppConfig());
        }

    }
}
