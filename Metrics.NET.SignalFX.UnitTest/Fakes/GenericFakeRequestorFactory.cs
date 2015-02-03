using Metrics.SignalFx.Helpers;

namespace Metrics.NET.SignalFX.UnitTest.Fakes
{
    public class GenericFakeRequestorFactory<TRequestor> : IWebRequestorFactory
        where TRequestor : IWebRequestor, new()
    {
        public IWebRequestor GetRequestor()
        {
            return new TRequestor();
        }
    }
}
