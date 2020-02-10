using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace saga
{
    public class MassTransitService : IHostedService
    {
        private readonly IBusControl bus;
        private readonly ILogger logger;

        public MassTransitService(IBusControl bus, ILoggerFactory loggerFactory)
        {
            this.bus = bus;
            this.logger = loggerFactory.CreateLogger<MassTransitService>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation(message: "starting bus...");

            return this.bus.StartAsync(cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation(message: "stopping bus...");

            return this.bus.StopAsync(cancellationToken: cancellationToken);
        }
    }
}