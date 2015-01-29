
using System.IO;

namespace Metrics.SignalFX.Helpers
{
    public interface IWebRequestor
    {
        Stream GetWriteStream(int contentLength);

        Stream Send();
    }
}
