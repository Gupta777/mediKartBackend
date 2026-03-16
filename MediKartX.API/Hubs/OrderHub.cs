using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MediKartX.API.Hubs;

public class OrderHub : Hub
{
    // Clients (shops) connect with ?shopId={id}
    public override Task OnConnectedAsync()
    {
        var http = Context.GetHttpContext();
        if (http != null && http.Request.Query.ContainsKey("shopId"))
        {
            var shopId = http.Request.Query["shopId"].ToString();
            ConnectionMapping.Add(shopId, Context.ConnectionId);
        }
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(System.Exception? exception)
    {
        var http = Context.GetHttpContext();
        if (http != null && http.Request.Query.ContainsKey("shopId"))
        {
            var shopId = http.Request.Query["shopId"].ToString();
            ConnectionMapping.Remove(shopId);
        }
        return base.OnDisconnectedAsync(exception);
    }

    // optional: shop can acknowledge via hub method
    public Task Acknowledge(string orderId)
    {
        // no-op for now
        return Task.CompletedTask;
    }
}

public static class ConnectionMapping
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _map = new();
    public static void Add(string key, string connectionId) => _map[key] = connectionId;
    public static void Remove(string key) => _map.TryRemove(key, out _);
    public static bool TryGet(string key, out string connectionId) => _map.TryGetValue(key, out connectionId);
}
