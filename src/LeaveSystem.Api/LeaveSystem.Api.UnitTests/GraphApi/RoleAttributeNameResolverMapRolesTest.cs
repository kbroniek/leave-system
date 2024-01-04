using LeaveSystem.Api.GraphApi;
using LeaveSystem.Shared;
using System.Text.Json;

namespace LeaveSystem.Api.UnitTests.GraphApi;

public class RoleAttributeNameResolverMapRolesTest
{
    [Theory]
    [MemberData(nameof(Get_WhenThereIsNoSuchRoleInAdditionalData_ThenReturnEmptyRolesAttribute_TestData))]
    public void WhenThereIsNoSuchRoleInAdditionalData_ThenReturnEmptyRolesAttribute(
        Dictionary<string, object>? additionalData)
    {
        //Given
        //When
        var rolesAttribute = RoleAttributeNameResolver.MapRoles(additionalData, TestData.FakeRoleAttributeName);
        rolesAttribute.Should().BeEquivalentTo(
            RolesResult.Empty,
            o => o.ComparingByMembers<RolesResult>());
    }

    public static IEnumerable<object?[]>
        Get_WhenThereIsNoSuchRoleInAdditionalData_ThenReturnEmptyRolesAttribute_TestData()
    {
        yield return new object?[] { new Dictionary<string, object>() };
        yield return new object[]
        {
            new Dictionary<string, object>
            {
                { "fakeKey12", new { } },
                { "fakeKey32", new { Prop = "fake prop" } }
            }
        };
        yield return new object?[] { null };
    }

    [Fact]
    public void WhenThereIsSuchRolesRawInAdditionalData_ThenReturnRolesAttributeWithRolesFromRaw()
    {
        //Given
        var fakeRolesAttribute = JsonSerializer.Deserialize<RolesResult>(TestData.FakeRolesJson);
        var additionalData = new Dictionary<string, object>
        {
            {
                "fakeKey",
                TestData.FakeRolesJsonV2
            },
            {
                TestData.FakeRoleAttributeName,
                TestData.FakeRolesJson
            },
            {
                "fakeKey1",
                TestData.FakeRolesJsonV2
            }
        };
        //When
        var rolesAttribute = RoleAttributeNameResolver.MapRoles(additionalData, TestData.FakeRoleAttributeName);
        rolesAttribute.Should().BeEquivalentTo(fakeRolesAttribute
        , o => o.ComparingByMembers<RolesResult>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    [InlineData(" ")]
    public void WhenRolesRawIsNullOrWhiteSpace_ThenReturnEmptyRolesAttribute(string? rolesRaw)
    {
        //Given
        var additionalData = new Dictionary<string, object>
        {
            {
                "fakeKey",
                TestData.FakeRolesJsonV2
            },
            {
                TestData.FakeRoleAttributeName,
                rolesRaw
            },
            {
                "fakeKey1",
                TestData.FakeRolesJsonV2
            }
        };
        //When
        var rolesAttribute = RoleAttributeNameResolver.MapRoles(additionalData, TestData.FakeRoleAttributeName);
        rolesAttribute.Should().BeEquivalentTo(
            RolesResult.Empty
            , o => o.ComparingByMembers<RolesResult>());
    }
}