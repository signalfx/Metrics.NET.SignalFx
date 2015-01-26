
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
        public static readonly int MAX_DATAPOINTS_PER_MESSAGE = 10000;
        private static readonly string INSTANCE_ID_DIMENSION = "InstanceId";

        private MetricsReports reports;
        private string apiToken;
        private TimeSpan interval;
        private IDictionary<string, string> defaultDimensions = new Dictionary<string, string>();
        private string baseURI = DEFAULT_URI;
        private int maxDatapointsPerMessage = MAX_DATAPOINTS_PER_MESSAGE;
        private string defaultSource;

        internal SignalFxReporterBuilder(MetricsReports reports, string apiToken, TimeSpan interval)
        {
            this.reports = reports;
            this.apiToken = apiToken;
            this.interval = interval;
        }

        public SignalFxReporterBuilder WithDefaultDimensions(IDictionary<string, string> defaultDimensions)
        {
            this.defaultDimensions = defaultDimensions;
            return this;
        }

        public SignalFxReporterBuilder WithBaseURI(String baseURI)
        {
            this.baseURI = baseURI;
            return this;
        }

        public SignalFxReporterBuilder WithMaxDatapointsPerMessage(int maxDatapointsPerMessage)
        {
            this.maxDatapointsPerMessage = maxDatapointsPerMessage;
            return this;
        }

        public SignalFxReporterBuilder WithNetBiosNameSource()
        {
            return WithSource(System.Environment.MachineName);
        }

        public SignalFxReporterBuilder WithDNSSource()
        {
            return WithSource(System.Net.Dns.GetHostName());
        }

        public SignalFxReporterBuilder WithFQDNSource()
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
        public SignalFxReporterBuilder WithSource(string defaultSource)
        {
            this.defaultSource = defaultSource;
            return this;
        }

        public MetricsReports Build()
        {
            return reports.WithReport(new SignalFxReport(new SignalFxReporter(baseURI, apiToken), defaultSource, defaultDimensions, maxDatapointsPerMessage), interval);
        }


    }
}
