using Metrics.Logging;
using Metrics.Reports;
using Metrics.SignalFx;
using Metrics.SignalFX.Configuration;
using System;
using System.Collections.Generic;

namespace Metrics
{
    public static class SignalFxExtensions
    {
        private static readonly ILog log = LogProvider.GetCurrentClassLogger();

        public static SignalFxReporterBuilder WithSignalFx(this MetricsReports reports, string apiToken, TimeSpan interval)
        {
            return new SignalFxReporterBuilder(reports, apiToken, interval);
        }

        public static MetricsReports WithSignalFxFromAppConfig(this MetricsReports reports)
        {
            try
            {
                SignalFxReporterConfiguration config = SignalFxReporterConfiguration.FromConfig();

                if (config == null)
                {
                    return reports;
                }

                IDictionary<string, string> defaultDimensions = new Dictionary<string, string>();
                if (config.DefaultDimensions != null)
                {
                    foreach (DefaultDimension defaultDimension in config.DefaultDimensions)
                    {
                        defaultDimensions.Add(defaultDimension.Name, defaultDimension.Value);
                    }
                }
                SignalFxReporterBuilder builder = new SignalFxReporterBuilder(reports, config.APIToken, config.SampleInterval);
                builder.WithBaseURI(config.BaseURI);
                builder.WithMaxDatapointsPerMessage(config.MaxDatapointsPerMessage);
                builder.WithDefaultDimensions(defaultDimensions);
                if (config.AwsIntegration)
                {
                    builder.WithAWSInstanceIdDimension();
                }
                switch (config.SourceType)
                {
                    case SourceType.netbios:
                        builder.WithNetBiosNameSource();
                        break;
                    case SourceType.dns:
                        builder.WithDNSSource();
                        break;
                    case SourceType.fqdn:
                        builder.WithFQDNSource();
                        break;
                    case SourceType.custom:
                        if (!string.IsNullOrEmpty(config.SourceValue))
                        {
                            builder.WithSource(config.SourceValue);
                            break;
                        }
                        throw new Exception("Metrics.SignalFx.Source.Value must be set if Metrics.SignalFx.Source.Type is \"source\".");
                    default:
                        throw new Exception("Metrics.SignalFx.Source.Type must be one of netbios, dns, fqdn, or source(with Metrics.SignalFx.Source.Value set)");
                }
                return builder.Build();
            }
            catch (Exception x)
            {
                log.ErrorException("Metrics: Error configuring SignalFx reports", x);
                throw new InvalidOperationException("Invalid Metrics Configuration: Metrics.SignalFx.APIToken must be non-empty and Metrics.SignalFx.Interval.Seconds must be an integer > 0", x);

            }
        }

    }
}
