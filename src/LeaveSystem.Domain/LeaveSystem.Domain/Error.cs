namespace LeaveSystem.Domain;

using System.Net;

public record struct Error(string Message, HttpStatusCode HttpStatusCode)
{
    public static Error Conflict(string resource) =>
        new($"The request could not be completed due to a conflict with the current state of the resource. {resource} Please resolve the conflict and try again.", HttpStatusCode.Conflict);
    public static Error BadRequest(string parameter) =>
        new($"Missing required fields or invalid input format. Please verify the request payload and parameters. [{parameter}]", HttpStatusCode.Conflict);
}
