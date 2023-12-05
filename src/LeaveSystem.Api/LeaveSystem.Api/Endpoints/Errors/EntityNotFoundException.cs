namespace LeaveSystem.Api.Endpoints.Errors;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message){}
}