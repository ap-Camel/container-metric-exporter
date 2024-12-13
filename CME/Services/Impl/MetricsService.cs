using OpenTelemetry.Metrics;
using OpenTelemetry;
using System.Diagnostics.Metrics;
using Docker.DotNet.Models;
using OpenTelemetry.Resources;

namespace CME.Services.Impl
{
    public class MetricsService : IMetricsService
    {
        private MeterProvider? _meterProvider;
        private Meter _meter = new("CME");
        private Dictionary<string, (double cpu, double mem)> _containerMetrics = new();
        private List<string> _containerIds = new();
        private Dictionary<string, ContainerListResponse> _containersInfo = new();
        private string OtelUrl;

        public MetricsService(string otelUrl)
        {
            OtelUrl = otelUrl;
        }

        public void Initialize()
        {
            _meterProvider = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("CME"))
                .AddMeter("CME")                
                .AddConsoleExporter()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(OtelUrl);
                })
                .Build();
        }

        public void SetContainers(List<string> containerIds)
        {
            _containerIds = containerIds;
            foreach (var id in containerIds)
            {
                _containerMetrics[id] = (0, 0);
            }

            _meter.CreateObservableGauge("docker_container_cpu_usage", () =>
            {
                return _containerMetrics.Select(kv => new Measurement<double>(kv.Value.cpu, new KeyValuePair<string, object?>("container_id", kv.Key)));
            });

            _meter.CreateObservableGauge("docker_container_memory_usage", () =>
            {
                return _containerMetrics.Select(kv => new Measurement<double>(kv.Value.mem, new KeyValuePair<string, object?>("container_id", kv.Key)));
            });
        }

        public void SetContainers(List<ContainerListResponse> containers)
        {
            _containerIds = containers.Select(x => x.ID).ToList();
            //foreach (var containerName in containers.Select(x => x.Names.FirstOrDefault("unknow_container")))
            //{
            //    _containerMetrics[containerName] = (0, 0);
            //}

            for(int i=0; i < containers.Count; i++)
            {
                _containersInfo[containers[i].ID] = containers[i];
                _containerMetrics[containers[i].Names.FirstOrDefault($"Unknow_container_{i}")] = (0, 0);
                //_containerMetrics[containers[i].ID] = (0, 0);
            }

            _meter.CreateObservableGauge("docker_container_cpu_usage", () =>
            {
                return _containerMetrics.Select(kv => new Measurement<double>(kv.Value.cpu, new KeyValuePair<string, object?>("container_name", kv.Key)));
            });

            _meter.CreateObservableGauge("docker_container_memory_usage", () =>
            {
                return _containerMetrics.Select(kv => new Measurement<double>(kv.Value.mem, new KeyValuePair<string, object?>("container_name", kv.Key)));
            });
        }

        public List<string> GetContainerIds() => _containerIds;
        public List<string> GetContainerNames() => _containerMetrics.Keys.ToList();
        public List<string> GetContainerIdsFromDict() => _containersInfo.Keys.ToList();
        public Dictionary<string, ContainerListResponse> GetContainerInfo() => _containersInfo;

        public void UpdateMetrics(string containerId, double cpuUsage, double memoryUsage)
        {
            if (_containerMetrics.ContainsKey(containerId))
            {
                _containerMetrics[containerId] = (cpuUsage, memoryUsage);
            }
        }

        public void UpdateMetricsByName(string containerName, double cpuUsage, double memoryUsage)
        {
            if (_containerMetrics.ContainsKey(containerName))
            {
                _containerMetrics[containerName] = (cpuUsage, memoryUsage);
            }
        }

        public void Dispose()
        {
            _meterProvider?.Dispose();
        }
    }
}
