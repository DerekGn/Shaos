using Microsoft.Extensions.Logging;
using Shaos.Sdk;

namespace Shaos.Test.PlugIn
{
    public class TestPlugIn : IPlugIn
    {
        private readonly ILogger<TestPlugIn> _logger;

        public TestPlugIn(ILogger<TestPlugIn> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting [{Name}].[{Operation}]", nameof(TestPlugIn), nameof(ExecuteAsync));

            do
            {
                await Task.Delay(100, cancellationToken);
                _logger.LogInformation("Executing [{Name}].[{Operation}]", nameof(TestPlugIn), nameof(ExecuteAsync));
            } while (cancellationToken.IsCancellationRequested);

            _logger.LogInformation("Completed [{Name}].[{Operation}]", nameof(TestPlugIn), nameof(ExecuteAsync));
        }
    }
}
