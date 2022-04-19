using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LeaveSystem.Exceptions;
public class HttpResponseException : Exception
{
    private HttpStatusCode internalServerError;

    public HttpResponseException(HttpStatusCode internalServerError)
    {
        this.internalServerError = internalServerError;
    }

    public HttpResponseException(string? message, HttpStatusCode internalServerError) : base(message)
    {
        this.internalServerError = internalServerError;
    }

    public HttpResponseException(string? message, Exception? innerException, HttpStatusCode internalServerError) : base(message, innerException)
    {
        this.internalServerError = internalServerError;
    }

    internal static Exception InternalServerError(string? message)
    {
        return new HttpResponseException(message, HttpStatusCode.InternalServerError);
    }

    internal static Exception NotFound(string? message)
    {
        return new HttpResponseException(message, HttpStatusCode.NotFound);
    }

    public static HttpResponseException BadRequest(string? message)
    {
        return new HttpResponseException(message, HttpStatusCode.BadRequest);
    }
}
