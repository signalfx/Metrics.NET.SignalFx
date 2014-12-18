
using com.signalfuse.metrics.protobuf;

namespace Metrics.SignalFx
{
    public interface ISignalFxReporter
    {
        void Send(DataPointUploadMessage msg);
    }
}
