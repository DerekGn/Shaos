/*
* MIT License
*
* Copyright (c) 2025 Derek Goslin https://github.com/DerekGn
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

namespace Shaos.Hosting
{
    public class MonitorHostedService : IHostedService
    {
        private readonly ILogger<MonitorHostedService> _logger;

        public MonitorHostedService(ILogger<MonitorHostedService> logger,
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
            _logger.LogInformation($"Background Worker [{nameof(MonitorHostedService)}] Starting");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Background Worker [{nameof(MonitorHostedService)}] Stopping");

            return Task.CompletedTask;
        }
    }
}