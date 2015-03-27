using System.Collections.Generic;
using Metrics.SignalFx;
using com.signalfuse.metrics.protobuf;

namespace Metrics.NET.SignalFx.UnitTest.Fakes
{
    internal class FakeSignalFxReporter : List<DataPointUploadMessage>, ISignalFxReporter
    {
        public void Send(DataPointUploadMessage msg)
        {
            // do nothing
            Add(msg);
        }
    }
}
