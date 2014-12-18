using com.signalfuse.metrics.protobuf;
using Metrics.Logging;
using System;
using System.IO;
using System.Net;

namespace Metrics.SignalFx
{
    public class SignalFxReporter : ISignalFxReporter
    {
        private static readonly ILog log = LogProvider.GetCurrentClassLogger();
        private string baseURI;
        private string apiToken;

        public SignalFxReporter(string baseURI, string apiToken)
        {
            this.baseURI = baseURI;
            this.apiToken = apiToken;
        }

        public void Send(DataPointUploadMessage msg)
        {
            var req = WebRequest.CreateHttp(baseURI + "/v2/datapoint");
            req.Method = "POST";
            req.ContentType = "application/x-protobuf";
            req.Headers.Add("X-SF-TOKEN: " + apiToken);
            req.ContentLength = msg.SerializedSize;
            req.Proxy = null;
            try
            {
                using (var rs = req.GetRequestStream())
                {
                    msg.WriteTo(rs);
                }

                using (var resp = (HttpWebResponse)req.GetResponse())
                {
                    if (resp.StatusCode == HttpStatusCode.Forbidden)
                    {
                        log.Error("API token for sending metrics to SignalFuse is invalid");
                    }
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        Stream stream2 = resp.GetResponseStream();
                        StreamReader reader2 = new StreamReader(stream2);
                        MetricsErrorHandler.Handle(new Exception(reader2.ReadToEnd()));
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is WebException)
                {
                    var webex = ex as WebException;
                    using (var exresp = (HttpWebResponse)webex.Response)
                    {
                        if (exresp != null)
                        {
                            Stream stream2 = exresp.GetResponseStream();
                            StreamReader reader2 = new StreamReader(stream2);
                            var errorStr = reader2.ReadToEnd();
                            log.Error(errorStr);
                        }
                    }
                }
                MetricsErrorHandler.Handle(ex, "Failed to send metrics");
            }
        }
    }
}
