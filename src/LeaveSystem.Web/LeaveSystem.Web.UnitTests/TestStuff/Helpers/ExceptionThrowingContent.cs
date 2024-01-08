namespace LeaveSystem.Web.UnitTests.TestStuff.Helpers;

using System.Net;

public class ExceptionThrowingContent : HttpContent
{
    private readonly Exception exception;
    public ExceptionThrowingContent(Exception exception) => this.exception = exception;

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => Task.FromException(this.exception);

    protected override bool TryComputeLength(out long length)
    {
        length = 0L;
        return false;
    }
}
