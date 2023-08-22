using LeaveSystem.Api.UnitTests.Stubs;
using Microsoft.Graph;

namespace LeaveSystem.Api.UnitTests.Providers;

public class GraphServiceUsersCollectionPageProvider
{
    public static IGraphServiceUsersCollectionPage Get(string roleAttributeName, string rolesJson)
    {
        return new GraphServiceUsersCollectionPageStub
        {
            new()
            {
                Id = "1",
                AdditionalData = new Dictionary<string, object>
                {
                    { "fakeKey", "fakeData" }
                },
                Mail = "fake@mail.com",
                DisplayName = "fake.tom"
            },
            new()
            {
                Id = "2",
                AdditionalData = new Dictionary<string, object>
                {
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
                    },
                    { roleAttributeName, rolesJson }
                },
                Mail = "fake1@mail.com",
                DisplayName = "fake.jack"
            },
            new()
            {
                Id = "3",
                AdditionalData = new Dictionary<string, object>
                {
                    { roleAttributeName, rolesJson }
                },
                Mail = "fake3@mail.com",
                DisplayName = "fake.bruce"
            },
        };
    }
}