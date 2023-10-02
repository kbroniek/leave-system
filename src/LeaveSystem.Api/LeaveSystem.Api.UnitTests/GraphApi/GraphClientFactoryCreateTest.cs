using LeaveSystem.Api.GraphApi;

namespace LeaveSystem.Api.UnitTests.GraphApi;

public class GraphClientFactoryCreateTest
{
    [Theory]
    [InlineData(null, "fakeClientId", "fakeSecret", new [] {"fakeScope1"})]
    [InlineData("fakeTenantId", null, "fakeSecret", new [] {"fakeScope1"})]
    [InlineData("fakeTenantId", "fakeClientId", null, new [] {"fakeScope1"})]
    [InlineData("fakeTenantId", "fakeClientId", "fakeSecret", null)]
    public void WhenTenantIdOrClientIdOrSecretOrScopesIsNull_ThenThrowArgumentNullException(
        string? tenantId, string? clientId, string? secret, string[]? scopes)
    {
        //When
        var act = () =>
        {
            GraphClientFactory.Create(tenantId, clientId, secret, scopes);
        };
        //Then
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("  ", "fakeClientId", "fakeSecret", new[] { "fakeScope1" })]
    [InlineData("fakeTenatId", "  ", "fakeSecret", new[] { "fakeScope1" })]
    [InlineData("fakeTenatId", "fakeClientId", "  ", new[] { "fakeScope1" })]
    [InlineData("fakeTenatId", "fakeClientId", "fakeSecret", new string[] { })]
    public void WhenTenantIdOrClientIdOrSecretIsWhiteSpaceOrScopesIsEmpty_ThenThrowArgumentException(
        string? tenantId, string? clientId, string? secret, string[]? scopes)
    {
        //When
        var act = () =>
        {
            GraphClientFactory.Create(tenantId, clientId, secret, scopes);
        };
        //Then
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WhenCreated_ThenGraphServiceClientHaveProperlyCreatedAttributes()
    {
        //Given
        const string tenantId = "fakeTenatId";
        const string clientId = "fakeClientId";
        const string secret = "fakeSecret";
        string[] scopes = { "fakeScp1", "fakeScp2" };
        var factory = GraphClientFactory.Create(tenantId, clientId, secret, scopes);
        //When
        var client = factory.Create();
        //Then
        client.Should().NotBeNull();
    }
}