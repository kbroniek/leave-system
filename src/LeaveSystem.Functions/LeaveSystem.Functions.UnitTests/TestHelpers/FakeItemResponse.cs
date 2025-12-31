namespace LeaveSystem.Functions.UnitTests.TestHelpers;

using Microsoft.Azure.Cosmos;

internal class FakeItemResponse<T> : ItemResponse<T>
{
    public FakeItemResponse() : base() { }
}
