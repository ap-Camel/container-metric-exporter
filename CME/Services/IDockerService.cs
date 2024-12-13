using Docker.DotNet.Models;

namespace CME.Services
{
    public interface IDockerService : IDisposable
    {
        Task<IList<ContainerListResponse>> ListContainersAsync(CancellationToken cancellationToken);
        Task<(double cpuUsage, double memoryUsage)> GetContainerMetricsAsync(string containerId, CancellationToken cancellationToken);
        Task<ContainerStatsResponse> GetContainerStatsAsync(string containerId, CancellationToken cancellationToken);
    }
}
