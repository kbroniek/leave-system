using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Moq;

namespace LeaveSystem.Api.UnitTests;

public static class EntityEntryMockFactory
{
    public static Mock<EntityEntry<T>> Create<T>() where T : class =>
        new(FormatterServices.GetUninitializedObject(typeof(InternalEntityEntry)));
}
