using System.Collections;
using Microsoft.Graph;

namespace LeaveSystem.Api.UnitTests.Stubs;

public class GraphServiceUsersCollectionPageStub : List<User>, IGraphServiceUsersCollectionPage
{
    bool ICollection<User>.IsReadOnly => false;
    IEnumerator<User> IEnumerable<User>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IList<User> CurrentPage => this;
    public IDictionary<string, object> AdditionalData { get; set; } = null!;

    public void InitializeNextPageRequest(IBaseClient client, string nextPageLinkString) => throw new NotImplementedException();

    public IGraphServiceUsersCollectionRequest NextPageRequest => null!;
}
