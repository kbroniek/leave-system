using LeaveSystem.GraphApi;

namespace LeaveSystem.UnitTests.GraphApi;

public class RoleAttributeNameResolverCreateTest
{
    [Fact]
    public void WhenB2CExtensionAppClientIdIsNull_ThenThrowArgumentNullException()
    {
        //Given
        string? fakeB2CExtensionAppClientId = null;
        //When
        var act = () => { RoleAttributeNameResolver.Create(fakeB2CExtensionAppClientId); };
        //Then
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenB2CExtensionAppClientIdIsEmpty_ThenThrowArgumentException()
    {
        //Given
        var fakeB2CExtensionAppClientId = string.Empty;
        //When
        var act = () => { RoleAttributeNameResolver.Create(fakeB2CExtensionAppClientId); };
        //Then
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WhenSuccessfullyCreated_ThenRoleAttributeNameIsSameAsPattern()
    {
        //Given
        var fakeB2CExtensionAppClientId = "fakeClientId";
        //When
        var resolver = RoleAttributeNameResolver.Create(fakeB2CExtensionAppClientId);
        //Then
        resolver.Should().BeEquivalentTo(new
        {
            RoleAttributeName = $"extension_{fakeB2CExtensionAppClientId}_Role"
        });
    }
}
