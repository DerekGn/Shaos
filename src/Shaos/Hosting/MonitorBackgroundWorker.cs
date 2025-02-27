

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
            _logger.LogInformation("Application Stopping");
        }

        private void OnStopped()
        {
            _logger.LogInformation("Application Stopped");
        }

        private void OnStarted()
        {
            _logger.LogInformation("Application Started");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Background Worker [{nameof(MonitorBackgroundWorker)}] Starting");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Background Worker [{nameof(MonitorBackgroundWorker)}] Stopping");

            return Task.CompletedTask;
        }
    }
}