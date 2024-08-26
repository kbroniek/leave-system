namespace LeaveSystem.Functions.UnitTests.testHelpers;

using LeaveSystem.Shared;

internal static class TestExtensions
{
    public static void ShouldBeFulted<TError>(this Result<TError> result, TError error)
    {
        result.IsOk.Should().BeFalse();
        return result.IfFaulted(e => e).Should().BeEquivalentTo();
    }
}
