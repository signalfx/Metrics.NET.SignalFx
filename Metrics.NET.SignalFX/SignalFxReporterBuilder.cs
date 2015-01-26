
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Metrics.Reports;

namespace Metrics.SignalFx
{
    public class SignalFxReporterBuilder
    {
        public static readonly string DEFAULT_URI = "https://api.signalfuse.com";
        private static readonly string INSTANCE_ID_DIMENSION = "InstanceId";

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
}
