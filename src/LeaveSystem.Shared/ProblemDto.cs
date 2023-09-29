namespace LeaveSystem.Shared;

public record ProblemDto(string Type, string Title, int Status, string Detail, string Instance, string Env, string Version);