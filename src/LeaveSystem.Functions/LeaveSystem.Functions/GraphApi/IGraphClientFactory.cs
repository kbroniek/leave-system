namespace LeaveSystem.Functions.GraphApi;
using Microsoft.Graph;

public interface IGraphClientFactory
{
    GraphServiceClient Create();
}
