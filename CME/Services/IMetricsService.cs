using Docker.DotNet.Models;

namespace CME.Services
{
    public interface IMetricsService : IDisposable
    {
        void Initialize();
        void SetContainers(List<string> containerIds);
        void SetContainers(List<ContainerListResponse> containers);
        List<string> GetContainerIds();
        List<string> GetContainerNames();
        List<string> GetContainerIdsFromDict();
        Dictionary<string, ContainerListResponse> GetContainerInfo();
        void UpdateMetrics(string containerId, double cpuUsage, double memoryUsage);
        void UpdateMetricsByName(string containerName, double cpuUsage, double memoryUsage);
    }
}
