using System.Collections;
using Microsoft.Graph;
using Moq;

namespace LeaveSystem.Api.UnitTests.Stubs;

public class GraphServiceUsersCollectionPageStub : List<User>, IGraphServiceUsersCollectionPage
{

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IList<User> CurrentPage => this;
    public IDictionary<string, object> AdditionalData { get; set; }

    public void InitializeNextPageRequest(IBaseClient client, string nextPageLinkString)
    {
        throw new NotImplementedException();
    }

    public IGraphServiceUsersCollectionRequest NextPageRequest => null;
}