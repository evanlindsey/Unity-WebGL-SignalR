using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

namespace SignalRServer.Hubs
{
    public class MainHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Connected: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public async Task SendPayloadAll(string payload)
        {
            var data = JsonSerializer.Deserialize<dynamic>(payload);
            string json = JsonSerializer.Serialize(data);
            await Clients.All.SendAsync("ReceivePayloadAll", json);
        }

        public async Task SendPayloadCaller(string payload)
        {
            var data = JsonSerializer.Deserialize<dynamic>(payload);
            string json = JsonSerializer.Serialize(data);
            await Clients.Caller.SendAsync("ReceivePayloadCaller", json);
        }
    }
}
