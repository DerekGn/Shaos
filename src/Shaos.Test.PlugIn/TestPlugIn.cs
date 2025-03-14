using Shaos.Sdk;

namespace Shaos.Test.PlugIn
{
    public class TestPlugIn : IPlugIn
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            do
            {
                await Task.Delay(100, cancellationToken);
            } while (cancellationToken.IsCancellationRequested);
        }
    }
}
