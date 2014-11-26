using Metrics.Reports;
using Metrics.SignalFx;
using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace Metrics
{
    public static class SignalFxExtensions
    {
        private static readonly string DEFAULT_URI = "https://api.signalfuse.com";

        public class SignalFxReporterBuilder
        {
            private MetricsReports reports;
            private string apiToken;
            private TimeSpan interval;
            private string baseURI;

            internal SignalFxReporterBuilder(MetricsReports reports, string apiToken, TimeSpan interval) : this(reports, apiToken, interval, DEFAULT_URI) { }

            internal SignalFxReporterBuilder(MetricsReports reports, string apiToken, TimeSpan interval, string baseURI)
            {
                this.reports = reports;
                this.apiToken = apiToken;
                this.interval = interval;
                this.baseURI = baseURI;
            }

            public MetricsReports WithNetBiosNameSource()
            {
                return WithSource(System.Environment.MachineName);
            }

            public MetricsReports WithDNSNameSource()
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

            public MetricsReports WithAWSInstanceIdSource()
            {
                var req = WebRequest.CreateHttp("http://169.254.169.254/latest/meta-data/instance-id");
                req.Method = "GET";
                req.Timeout = 1000 * 60;
                using (var resp = (HttpWebResponse)req.GetResponse())
                {
                    string source = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                    return WithSource(source);
                }
            }

            public MetricsReports WithSource(string defaultSource)
            {
                return reports.WithReport(new SignalFxReport(new SignalFxReporter(baseURI, apiToken), defaultSource), interval);
            }
        }

        public static SignalFxReporterBuilder WithSignalFx(this MetricsReports reports, string apiToken, TimeSpan interval)
        {
            return new SignalFxReporterBuilder(reports, apiToken, interval);
        }

        public static SignalFxReporterBuilder WithSignalFx(this MetricsReports reports, string apiToken, string defaultSource, string baseURI, TimeSpan interval)
        {
            return new SignalFxReporterBuilder(reports, apiToken, interval, baseURI);
        }

    }
}
