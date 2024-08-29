namespace LeaveSystem.Domain;

using System.Net;

public record struct Error(string Message, HttpStatusCode HttpStatusCode);
