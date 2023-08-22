using Microsoft.Graph;

namespace LeaveSystem.Api.GraphApi;

public interface IGraphClientFactory
{
    GraphServiceClient Create();
}