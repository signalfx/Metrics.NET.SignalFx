
using System;
using System.IO;
using System.Net;
using System.Security;

namespace Metrics.SignalFx.Helpers
{
    public class WebRequestor : IWebRequestor
    {
        public WebRequestor(string uri)
        {
            _request = WebRequest.CreateHttp(uri);
        }

        public WebRequestor WithMethod(string method)
        {
            _request.Method = method;
            return this;
        }

        public WebRequestor WithContentType(string contentType)
        {
            _request.ContentType = contentType;
            return this;
        }

        public WebRequestor WithHeader(string name, string value)
        {
            _request.Headers.Add(name, value);
            return this;
        }
        
        public Stream GetWriteStream(int contentLength)
        {
            _request.ContentLength = contentLength;
            return _request.GetRequestStream();
        }

        public Stream Send()
        {
            _response = (HttpWebResponse)_request.GetResponse();
            if (_response.StatusCode != HttpStatusCode.OK)
            {
                if (_response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new SecurityException("HTTP 403: " + _response.StatusDescription);
                }
                throw new WebException(_response.StatusDescription, null, WebExceptionStatus.UnknownError, _response);
            }
            return _response.GetResponseStream();
        }

        public WebRequestor WithTimeout(int timeoutInMilliseconds)
        {
            _request.Timeout = timeoutInMilliseconds;
            return this;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool isFinalizer)
        {
            _request.Abort();
            if (_response != null)
            {
                _response.Dispose();
            }
        }

        private HttpWebResponse _response;
        private HttpWebRequest _request;
    }
}
