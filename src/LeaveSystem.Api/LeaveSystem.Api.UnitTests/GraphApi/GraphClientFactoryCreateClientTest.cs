using GraphClientFactory = LeaveSystem.Api.GraphApi.GraphClientFactory;

namespace LeaveSystem.Api.UnitTests.GraphApi;

public class GraphClientFactoryCreateClientTest
{
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
        //Todo: Make correct assertion
    }
}
