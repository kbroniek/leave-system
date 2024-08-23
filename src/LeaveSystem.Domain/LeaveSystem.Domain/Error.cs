namespace LeaveSystem.Domain;

using System.Net;

public record Error(string Message, HttpStatusCode HttpStatusCode);
