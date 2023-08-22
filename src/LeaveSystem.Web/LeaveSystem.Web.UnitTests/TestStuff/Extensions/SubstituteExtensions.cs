using NSubstitute;
using NSubstitute.Core;

namespace LeaveSystem.Web.UnitTests.TestStuff.Extensions;

public static class SubstituteExtensions
{
    public static ConfiguredCall ReturnsAsync<T>(this Task<T> value, T returnThis)
    {
        return value.Returns(returnThis);
    }
}