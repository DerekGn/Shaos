

namespace Shaos.Hosting
{
    public class MonitorBackgroundWorker : IHostedService
    {
        private readonly ILogger<MonitorBackgroundWorker> _logger;
        
        public MonitorBackgroundWorker(
            ILogger<MonitorBackgroundWorker> logger,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
            hostApplicationLifetime.ApplicationStopped.Register(OnStopped);
            hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
        }

        private void OnStopping()
        {
            throw new NotImplementedException();
        }

        private void OnStopped()
        {
            throw new NotImplementedException();
        }

        private void OnStarted()
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}