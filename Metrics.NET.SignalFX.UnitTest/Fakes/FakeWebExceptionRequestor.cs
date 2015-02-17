
using System;
using System.IO;
using System.Net;
using System.Security;
using System.Text;
using Metrics.SignalFx.Helpers;

namespace Metrics.NET.SignalFX.UnitTest.Fakes
{
    public class FakeWebExceptionRequestor : IWebRequestor
    {
        public Stream GetWriteStream(int contentLength)
        {
            return new MemoryStream();
        }

        public Stream Send()
        {
            WebResponse resp = new FakeWebResponse("text/html", "<html><body><h1>Y U No Work?</h1></body></html>");

            throw new WebException(
                "Sump'n happened",
                new Exception(),
                WebExceptionStatus.UnknownError,
                resp
                );
        }

        public void Dispose()
        {
        }
    }
}
