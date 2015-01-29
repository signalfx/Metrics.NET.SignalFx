
using System.IO;
using System.Security;
using System.Text;
using Metrics.SignalFX.Helpers;

namespace Metrics.NET.SignalFX.UnitTest.Fakes
{
    public class FakeAccessDeniedRequestor : IWebRequestor
    {
        public Stream GetWriteStream()
        {
            return new MemoryStream();
        }

        public Stream Send()
        {
            throw new SecurityException("Y U No Send good API Key!");
        }
    }
}
