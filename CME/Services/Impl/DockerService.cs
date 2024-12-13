using Docker.DotNet.Models;
using Docker.DotNet;

namespace CME.Services.Impl
{
    internal class DockerService : IDockerService
    {
        private readonly DockerClient _client;

        public DockerService(Uri dockerUri)
        {
            var config = new DockerClientConfiguration(dockerUri);
            _client = config.CreateClient();
        }

        public async Task<IList<ContainerListResponse>> ListContainersAsync(CancellationToken cancellationToken)
        {
            return await _client.Containers.ListContainersAsync(new ContainersListParameters { All = true }, cancellationToken);
        }

        public async Task<(double cpuUsage, double memoryUsage)> GetContainerMetricsAsync(string containerId, CancellationToken cancellationToken)
        {
            var statsProgress = new StatsProgress();
            await _client.Containers.GetContainerStatsAsync(containerId, new ContainerStatsParameters { Stream = false }, statsProgress, cancellationToken);

            if (statsProgress.Response == null)
                return (0, 0);

            var stats = statsProgress.Response;
            var totalCpu = (double)stats.CPUStats.CPUUsage.TotalUsage;
            var memUsage = (double)stats.MemoryStats.Usage;
            return (totalCpu, memUsage);
        }

        public async Task<ContainerStatsResponse> GetContainerStatsAsync(string containerId, CancellationToken cancellationToken)
        {
            var statsProgress = new StatsProgress();
            await _client.Containers.GetContainerStatsAsync(containerId, new ContainerStatsParameters { Stream = false }, statsProgress, cancellationToken);

            if (statsProgress.Response == null)
                return new ContainerStatsResponse();

            var stats = statsProgress.Response;
            return stats;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        private class StatsProgress : IProgress<ContainerStatsResponse>
        {
            public ContainerStatsResponse? Response { get; private set; }
            public void Report(ContainerStatsResponse value) => Response = value;
        }
    }
}
