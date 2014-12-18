using Metrics.Logging;
using Metrics.Reports;
using Metrics.SignalFx;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;

namespace Metrics
{
    public static class SignalFxExtensions
    {
        private static readonly ILog log = LogProvider.GetCurrentClassLogger();

        public static SignalFxReporterBuilder WithSignalFx(this MetricsReports reports, string apiToken, TimeSpan interval)
        {
            return new SignalFxReporterBuilder(reports, apiToken, interval);
        }

        public static SignalFxReporterBuilder WithSignalFx(this MetricsReports reports, string apiToken, IDictionary<string, string> defaultDimensions, TimeSpan interval)
        {
            return new SignalFxReporterBuilder(reports, apiToken, defaultDimensions, interval);
        }
        public static SignalFxReporterBuilder WithSignalFx(this MetricsReports reports, string apiToken, string baseURI, TimeSpan interval)
        {
            return new SignalFxReporterBuilder(reports, apiToken, interval, baseURI);
        }

        public static MetricsReports WithSignalFxFromAppConfig(this MetricsReports reports)
        {
            try
            {
                var apiToken = ConfigurationManager.AppSettings["Metrics.SignalFx.APIToken"];
                var signalFxMetricsInterval = ConfigurationManager.AppSettings["Metrics.SignalFx.Interval.Seconds"];
                var signalFxSourceType = ConfigurationManager.AppSettings["Metrics.SignalFx.Source.Type"];
                var signalFxSourceValue = ConfigurationManager.AppSettings["Metrics.SignalFx.Source.Value"];
                var signalFxAWS = ConfigurationManager.AppSettings["Metrics.SignalFx.AWSIntegration"];
                if (!string.IsNullOrEmpty(apiToken) && !string.IsNullOrEmpty(signalFxMetricsInterval) && !string.IsNullOrEmpty(signalFxSourceType))
                {
                    int seconds;
                    if (int.TryParse(signalFxMetricsInterval, out seconds) && seconds > 0)
                    {
                        var defaultDimensionsHash = (ConfigurationManager.GetSection("SignalFx/DefaultDimensions") as System.Collections.Hashtable);
                        IDictionary<string, string> defaultDimensions;
                        if (defaultDimensionsHash != null)
                        {
                            defaultDimensions = defaultDimensionsHash.Cast<System.Collections.DictionaryEntry>()
                            .ToDictionary(n => n.Key.ToString(), n => n.Value.ToString());
                        }
                        else
                        {
                            defaultDimensions = new Dictionary<string, string>();
                        }

                        SignalFxReporterBuilder builder = new SignalFxReporterBuilder(reports, apiToken, defaultDimensions, TimeSpan.FromSeconds(seconds));
                        if  (!string.IsNullOrEmpty(signalFxAWS) && signalFxAWS == "true") {
                            builder = builder.WithAWSInstanceIdDimension();
                        }
                        switch (signalFxSourceType)
                        {
                            case "netbios":
                                return builder.WithNetBiosNameSource();
                            case "dns":
                                return builder.WithDNSSource();
                            case "fqdn":
                                return builder.WithFQDNSource();
                            case "custom":
                                if (!string.IsNullOrEmpty(signalFxSourceValue))
                                {
                                    return builder.WithSource(signalFxSourceValue);
                                }
                                throw new Exception("Metrics.SignalFx.Source.Value must be set if Metrics.SignalFx.Source.Type is \"source\".");
                            default:
                                throw new Exception("Metrics.SignalFx.Source.Type must be one of netbios, dns, fqdn, or source(with Metrics.SignalFx.Source.Value set)");
                        }
                    }
                }
                return reports;
            }
            catch (Exception x)
            {
                log.ErrorException("Metrics: Error configuring SignalFx reports", x);
                throw new InvalidOperationException("Invalid Metrics Configuration: Metrics.SignalFx.APIToken must be non-empty and Metrics.SignalFx.Interval.Seconds must be an integer > 0", x);

            }
        }

    }
}
