using System;
using System.Net;

namespace SimpleSyncPlugin.Exceptions
{
    public class ManualSynchronizationRequiredException : Exception
    {
    }

    public class ForceFetchRequiredException : Exception
    {
    }

    public class HttpStatusException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public HttpStatusException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}