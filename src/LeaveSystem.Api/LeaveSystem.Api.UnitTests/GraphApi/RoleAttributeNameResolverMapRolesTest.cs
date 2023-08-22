using System.Runtime.InteropServices;
using System.Text.Json;
using FluentAssertions;
using LeaveSystem.Api.GraphApi;
using LeaveSystem.Shared;
using Microsoft.Graph;

namespace LeaveSystem.Api.UnitTests.GraphApi;

public class RoleAttributeNameResolverMapRolesTest
{
    [Theory]
    [MemberData(nameof(Get_WhenThereIsNoSuchRoleInAdditionalData_ThenReturnEmptyRolesAttribute_TestData))]
    public void WhenThereIsNoSuchRoleInAdditionalData_ThenReturnEmptyRolesAttribute(
        Dictionary<string, object>? additionalData)
    {
        //Given
        var roleAttributeName = "fakeAttrName";
        //When
        var rolesAttribute = RoleAttributeNameResolver.MapRoles(additionalData, roleAttributeName);
        rolesAttribute.Should().BeEquivalentTo(
            RolesAttribute.Empty,
            o => o.ComparingByMembers<RolesAttribute>());
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
        const string roleAttributeName = "fakeAttrName";
        const string rolesJson = """
            {
              "Roles": [
                "nulla",
                "aliquip",
                "amet",
                "aliqua",
                "magna",
                "cillum",
                "excepteur"
              ]
            }
            """;
        var fakeRolesAttribute = JsonSerializer.Deserialize<RolesAttribute>(rolesJson);
        var additionalData = new Dictionary<string, object>
        {
            {
                "fakeKey",
                """
                {
                  "Roles": [
                    "enim",
                    "ut",
                    "et",
                    "aliquip",
                    "enim",
                    "aute",
                    "et"
                  ]
                }
                """
            },
            {
                roleAttributeName,
                rolesJson
            },
            {
                "fakeKey1",
                """
                {
                  "Roles": [
                    "enim",
                    "ut",
                    "et",
                    "aliquip",
                    "enim",
                    "aute",
                    "et"
                  ]
                }
                """
            }
        };
        //When
        var rolesAttribute = RoleAttributeNameResolver.MapRoles(additionalData, roleAttributeName);
        rolesAttribute.Should().BeEquivalentTo(fakeRolesAttribute
        , o => o.ComparingByMembers<RolesAttribute>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    [InlineData(" ")]
    public void WhenRolesRawIsNullOrWhiteSpace_ThenReturnEmptyRolesAttribute(string? rolesRaw)
    {
        //Given
        const string roleAttributeName = "fakeAttrName";
        var additionalData = new Dictionary<string, object>
        {
            {
                "fakeKey",
                """
                {
                  "Roles": [
                    "enim",
                    "ut",
                    "et",
                    "aliquip",
                    "enim",
                    "aute",
                    "et"
                  ]
                }
                """
            },
            {
                roleAttributeName,
                rolesRaw
            },
            {
                "fakeKey1",
                """
                {
                  "Roles": [
                    "enim",
                    "ut",
                    "et",
                    "aliquip",
                    "enim",
                    "aute",
                    "et"
                  ]
                }
                """
            }
        };
        //When
        var rolesAttribute = RoleAttributeNameResolver.MapRoles(additionalData, roleAttributeName);
        rolesAttribute.Should().BeEquivalentTo(
            RolesAttribute.Empty
            , o => o.ComparingByMembers<RolesAttribute>());
    }
}