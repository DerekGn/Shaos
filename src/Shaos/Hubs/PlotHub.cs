using Microsoft.AspNetCore.SignalR;

namespace Shaos.Hubs
{
    public class PlotHub : Hub<IPlotClient>
    {
        private readonly ILogger<PlotHub> _logger;

        public PlotHub(ILogger<PlotHub> logger)
        {
            _logger = logger;
        }

        public async Task UpdateAsync()
            => await Clients.All.UpdateAsync();

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                _logger.LogWarning(exception, "An exception occurred connection disconnected");
            }

            return base.OnDisconnectedAsync(exception);
        }
    }

}
