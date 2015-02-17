
using System;
using System.IO;

namespace Metrics.SignalFx.Helpers
{
    public interface IWebRequestor : IDisposable
    {
        Stream GetWriteStream(int contentLength);

        Stream Send();
    }
}
