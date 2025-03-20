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
            do
            {
                await Task.Delay(100, cancellationToken);
            } while (cancellationToken.IsCancellationRequested);
        }
    }
}
