using Microsoft.AspNetCore.SignalR;

namespace SignalRServer.Hubs;

public interface IMainHubClient
{
    Task ReceivePayloadAll(string payload);
    Task ReceivePayloadCaller(string payload);
}

public class MainHub(ILogger<MainHub> logger) : Hub<IMainHubClient>
{
    public override Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is not null)
        {
            logger.LogWarning(exception, "Client disconnected with error: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        }
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendPayloadAll(string payload)
    {
        logger.LogDebug("Broadcasting payload from {ConnectionId}", Context.ConnectionId);
        await Clients.All.ReceivePayloadAll(payload);
    }

    public async Task SendPayloadCaller(string payload)
    {
        logger.LogDebug("Sending payload to caller {ConnectionId}", Context.ConnectionId);
        await Clients.Caller.ReceivePayloadCaller(payload);
    }
}
