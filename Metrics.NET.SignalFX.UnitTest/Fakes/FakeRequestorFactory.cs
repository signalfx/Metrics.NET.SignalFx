
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.ProtocolBuffers;
using Metrics.SignalFx.Helpers;

namespace Metrics.NET.SignalFX.UnitTest.Fakes
{
    public class FakeRequestorFactory : IWebRequestorFactory
    {
        public FakeRequestorFactory()
        {
            _requestors = new List<FakeRequestor>();
        }

        public IWebRequestor GetRequestor()
        {
            var req = new FakeRequestor();
            _requestors.Add(req);
            return req;
        }

        public byte[] WrittenData
        {
            get
            {
                var data = new byte[_requestors.Sum(req => req.WrittenData != null ? req.WrittenData.Length : 0)];
                _requestors.Aggregate(0,
                    (i, requestor) =>
                    {
                        Array.Copy(requestor.WrittenData, 0, data, i, requestor.WrittenData.Length);
                        return i + requestor.WrittenData.Length;
                    });
                return data;
            }
        }

        private IList<FakeRequestor> _requestors;
    }
}
