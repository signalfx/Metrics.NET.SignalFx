using System.Security;
using com.signalfuse.metrics.protobuf;
using Metrics.Logging;
using System;
using System.IO;
using System.Net;
using Metrics.SignalFx.Helpers;

namespace Metrics.SignalFx
{
    public class SignalFxReporter : ISignalFxReporter
    {
        private static readonly ILog log = LogProvider.GetCurrentClassLogger();
        private string apiToken;
        private IWebRequestor _requestor;

        public SignalFxReporter(string baseURI, string apiToken, IWebRequestor requestor = null)
        {
            if (requestor == null)
            {
                requestor = new WebRequestor(baseURI + "/v2/datapoint")
                    .WithMethod("POST")
                    .WithContentType("application/x-protobuf")
                    .WithHeader("X-SF-TOKEN", apiToken);
            }

            this._requestor = requestor;
            this.apiToken = apiToken;
        }

        public void Send(DataPointUploadMessage msg)
        {
            try
            {
                using (var rs = _requestor.GetWriteStream(msg.SerializedSize))
                {
                    msg.WriteTo(rs);
                    // flush the message before disposing
                    rs.Flush();
                }
                try
                {
                    using (_requestor.Send())
                    {
                    }
                }
                catch (SecurityException)
                {
                    log.Error("API token for sending metrics to SignalFuse is invalid");
                }
            }
            catch (Exception ex)
            {
                if (ex is WebException)
                {
                    var webex = ex as WebException;
                    using (var exresp = webex.Response)
                    {
                        if (exresp != null)
                        {
                            var stream2 = exresp.GetResponseStream();
                            var reader2 = new StreamReader(stream2);
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
