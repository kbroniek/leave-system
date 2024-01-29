using Microsoft.Graph;

namespace LeaveSystem.GraphApi;

public interface IGraphClientFactory
{
    GraphServiceClient Create();
}