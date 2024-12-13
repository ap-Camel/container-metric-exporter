using CME.Services;
using CME.Services.Impl;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;

namespace CME
{
    public class ExporterApp
    {
        private readonly IDockerService _dockerService;
        private readonly IMetricsService _metricsService;

        public ExporterApp(IConfiguration configuration)
        {
            var dockerUri = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? configuration["Docker:UriWindows"] ?? "npipe://./pipe/docker_engine"
                : configuration["Docker:UriLinux"] ?? "unix:///var/run/docker.sock";

            _dockerService = new DockerService(new Uri(dockerUri));
            _metricsService = new MetricsService(configuration["OpenTelemetry:CollectorUrl"] ?? "http://127.0.0.1:4317");
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Starting Docker Metrics Exporter...");

            _metricsService.Initialize();
            await RefreshContainersAsync(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                await UpdateMetricsAsync(cancellationToken);

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            Console.WriteLine("Shutting down...");
            _dockerService.Dispose();
            _metricsService.Dispose();
        }

        private async Task RefreshContainersAsync(CancellationToken cancellationToken)
        {
            var containers = await _dockerService.ListContainersAsync(cancellationToken);
            if(containers != null && containers.Any())
            {
                //_metricsService.SetContainers(containers.Select(c => c.Names.FirstOrDefault("unknown")).ToList());
                _metricsService.SetContainers(containers.ToList());
            }
            
            Console.WriteLine($"Found {containers?.Count} containers for metrics collection.");
        }

        private async Task UpdateMetricsAsync(CancellationToken cancellationToken)
        {
            //var containerIds = _metricsService.GetContainerIds();
            //var containerNames = _metricsService.GetContainerNames();
            var containers = _metricsService.GetContainerInfo();

            int counter = 0;
            foreach (var container in containers)
            {
                //var stats = await _dockerService.GetContainerMetricsAsync(container.Key, cancellationToken);
                //_metricsService.UpdateMetrics(containerId, stats.cpuUsage, stats.memoryUsage);
                var stats = await _dockerService.GetContainerStatsAsync(container.Key, cancellationToken);
                _metricsService.UpdateMetricsByName(container.Value.Names.FirstOrDefault($"Unknow_container_{counter}"), 
                                                    stats.CPUStats.CPUUsage.UsageInKernelmode, 
                                                    stats.MemoryStats.Usage);
                counter++;
            }
        }
    }
}
