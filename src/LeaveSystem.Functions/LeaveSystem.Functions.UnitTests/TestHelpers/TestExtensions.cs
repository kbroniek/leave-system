namespace LeaveSystem.Functions.UnitTests.TestHelpers;

using LeaveSystem.Shared;

internal static class TestExtensions
{
    public static void ShouldBeFulted<TError>(this Result<TError> result, TError error)
    {
        result.IsSuccess.Should().BeFalse();
        return result.IfFaulted(e => e).Should().BeEquivalentTo();
    }
}
