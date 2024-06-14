namespace LeaveSystem.Functions.UnitTests.testHelpers;

using Microsoft.Azure.Cosmos;

internal class FakeItemResponse<T> : ItemResponse<T>
{
    public FakeItemResponse() : base() { }
}
