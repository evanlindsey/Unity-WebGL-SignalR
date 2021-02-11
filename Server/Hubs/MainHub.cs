using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace SignalRServer.Hubs
{
    public class MainHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public async Task SendPayloadA(string payload)
        {
            var msg = "We are in A...";
            Console.WriteLine(msg);
            List<object> objects = new List<object>(){msg};
            string json = JsonSerializer.Serialize(objects);
            // var data = JsonSerializer.Deserialize<dynamic>(payload);
            // string json = JsonSerializer.Serialize(data);
            await Clients.All.SendAsync("ReceivePayloadA", json);
        }

        public async Task SendPayloadB(string payload)
        {
            var data = JsonSerializer.Deserialize<dynamic>(payload);
            string json = JsonSerializer.Serialize(data);
            await Clients.All.SendAsync("ReceivePayloadB", json);
        }

    }
}
