using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;

namespace log
{
    public class BusService : IHostedService
    {
        private readonly IBusControl busControl;

        public BusService(IBusControl busControl)
        {
            this.busControl = busControl;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return this.busControl.StartAsync(cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return this.busControl.StopAsync(cancellationToken: cancellationToken);
        }
    }
}