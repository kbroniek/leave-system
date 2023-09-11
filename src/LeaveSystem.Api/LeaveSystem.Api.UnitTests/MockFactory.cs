using System.Reflection;
using Moq;

namespace LeaveSystem.Api.UnitTests;

public static class MockFactory
{
    public static Mock<T> CreateMock<T>() where T : class
    {
        return new Mock<T>();
    }
}