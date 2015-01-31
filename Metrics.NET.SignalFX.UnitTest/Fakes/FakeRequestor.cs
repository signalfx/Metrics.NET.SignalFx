
using System.IO;
using System.Text;
using Metrics.SignalFx.Helpers;

namespace Metrics.NET.SignalFX.UnitTest.Fakes
{
    public class FakeRequestor : IWebRequestor
    {
        public FakeRequestor()
        {
            _writeStream = new MemoryStream();
        }

        public byte[] WrittenData
        {
            get { return _writeStream.GetBuffer(); }
        }

        public string ResponseData { get; set; }

        public Stream GetWriteStream(int contentLength)
        {
            return _writeStream;
        }

        public Stream Send()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(ResponseData));
        }

        private readonly MemoryStream _writeStream;
    }
}
