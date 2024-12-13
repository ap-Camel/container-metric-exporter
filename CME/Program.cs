using Microsoft.Extensions.Configuration;

namespace CME
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

            var cancelSource = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cancelSource.Cancel();
            };

            var app = new ExporterApp(config);
            await app.RunAsync(cancelSource.Token);
        }
    }
}
