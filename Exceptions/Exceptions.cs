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

    public class AuthException : Exception
    {
        public string StatusCode { get; private set; }

        public AuthException(string statusCode) : base(statusCode)
        {
            StatusCode = statusCode;
        }
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