namespace LeaveSystem.Web.UnitTests.TestStuff.Helpers;

public class FakeException : Exception
{
    private readonly string fakeStackTrace;

    public FakeException(string message, string fakeStackTrace) : base(message) => this.fakeStackTrace = fakeStackTrace;

    public override string? StackTrace => this.fakeStackTrace;
}
