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

using Shaos.Services;
using Shaos.Services.Logging;

namespace Shaos.Startup
{
    public class InitialisationHostService : IHostedService
    {
        private readonly ILogger<InitialisationHostService> _logger;
        private readonly IServiceProvider _services;

        public InitialisationHostService(ILogger<InitialisationHostService> logger,
                                         IServiceProvider services)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initialising Application");

            await InitialiseLoggingConfiguration(cancellationToken);

            await InitialiseInstanceHostAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task InitialiseInstanceHostAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initialising Instance Host");

            using var scope = _services.CreateScope();

            var instanceHostService = scope.ServiceProvider
                .GetRequiredService<IInstanceHostService>();

            await instanceHostService.StartInstancesAsync(cancellationToken);
        }

        private async Task InitialiseLoggingConfiguration(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initialising Logging Configuration");

            using var scope = _services.CreateScope();

            var loggingConfiguration = scope
                .ServiceProvider
                .GetRequiredService<ILoggingConfiguration>();

            var loggingConfigurationService = scope
                .ServiceProvider
                .GetRequiredService<ILoggingConfigurationService>();

            await loggingConfigurationService.InitialiseLoggingConfigurationAsync(loggingConfiguration,
                                                                                  cancellationToken);
        }
    }
}