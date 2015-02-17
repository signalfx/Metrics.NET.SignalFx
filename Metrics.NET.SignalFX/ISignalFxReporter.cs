
using com.signalfuse.metrics.protobuf;
using Metrics.SignalFx.Helpers;

namespace Metrics.SignalFx
{
    /// <summary>
    /// Base interface for the reporter that actually transmits reporting data
    /// </summary>
    public interface ISignalFxReporter
    {
        /// <summary>
        /// Report the upload message
        /// </summary>
        /// <param name="msg">The message to report</param>
        void Send(DataPointUploadMessage msg);
    }
}
