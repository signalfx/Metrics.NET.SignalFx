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
        private static readonly string DEFAULT_URI = "https://api.signalfuse.com";
        private static readonly string INSTANCE_ID_DIMENSION = "InstanceId";
        private static readonly ILog log = LogProvider.GetCurrentClassLogger();

        public class SignalFxReporterBuilder
        {
            private MetricsReports reports;
            private string apiToken;
            private IDictionary<string, string> defaultDimensions;
            private TimeSpan interval;
            private string baseURI;

            internal SignalFxReporterBuilder(MetricsReports reports, string apiToken, TimeSpan interval) : this(reports, apiToken, new Dictionary<string, string>(), interval) { }

            internal SignalFxReporterBuilder(MetricsReports reports, string apiToken, IDictionary<string, string> defaultDimensions, TimeSpan interval) : this(reports, apiToken, defaultDimensions, interval, DEFAULT_URI) { }

            internal SignalFxReporterBuilder(MetricsReports reports, string apiToken, TimeSpan interval, string baseURI) : this(reports, apiToken, new Dictionary<string, string>(), interval, baseURI) { }

            internal SignalFxReporterBuilder(MetricsReports reports, string apiToken, IDictionary<string, string> defaultDimensions, TimeSpan interval, string baseURI)
            {
                this.reports = reports;
                this.apiToken = apiToken;
                this.defaultDimensions = defaultDimensions;
                this.interval = interval;
                this.baseURI = baseURI;
            }

            public MetricsReports WithNetBiosNameSource()
            {
                return WithSource(System.Environment.MachineName);
            }

            public MetricsReports WithDNSSource()
            {
                return WithSource(System.Net.Dns.GetHostName());
            }

            public MetricsReports WithFQDNSource()
            {
                string domainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
                string hostName = System.Net.Dns.GetHostName();

                if (!hostName.EndsWith(domainName))  // if hostname does not already include domain name
                {
                    hostName += "." + domainName;   // add the domain name part
                }
                return WithSource(hostName);
            }

            public SignalFxReporterBuilder WithAWSInstanceIdDimension()
            {
                var req = WebRequest.CreateHttp("http://169.254.169.254/latest/meta-data/instance-id");
                req.Method = "GET";
                req.Timeout = 1000 * 60;
                using (var resp = (HttpWebResponse)req.GetResponse())
                {
                    string source = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                    defaultDimensions[INSTANCE_ID_DIMENSION] = source;
                    return this;
                }
            }

            public MetricsReports WithSource(string defaultSource)
            {
                return reports.WithReport(new SignalFxReport(new SignalFxReporter(baseURI, apiToken), defaultSource, defaultDimensions), interval);
            }
        }

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
