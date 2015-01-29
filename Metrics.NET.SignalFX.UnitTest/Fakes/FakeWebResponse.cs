
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Metrics.NET.SignalFX.UnitTest.Fakes
{
    public class FakeWebResponse : WebResponse
    {
        public FakeWebResponse(string contentType, string content)
        {
            _contentType = contentType;
            _content = content;
        }

        public override string ContentType
        {
            get { return _contentType; }
        }

        public override long ContentLength
        {
            get { return Encoding.UTF8.GetByteCount(_content); }
        }

        public override WebHeaderCollection Headers
        {
            get { return new WebHeaderCollection(); }
        }

        public override Uri ResponseUri
        {
            get { return new Uri("http://fake.signalfuse.com/aasdfava"); }
        }

        public override Stream GetResponseStream()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(_content));
        }

        private readonly string _contentType;
        private readonly string _content;
    }
}
