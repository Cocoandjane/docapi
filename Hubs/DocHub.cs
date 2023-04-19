
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.SignalR;

namespace DocShareAPI.Hubs;

public class DocHub : Hub
{
    public override Task OnConnectedAsync()
    {
        Console.WriteLine("A Client Connected: " + Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine("A client disconnected: " + Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }


    public async Task JoinDoc(int docId)
    {
        Console.WriteLine($"Joining Doc: {docId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, docId.ToString());
    }

    public async Task LeaveDoc(int docId)
    {
        Console.WriteLine($"Leaving Doc: {docId}");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, docId.ToString());
    }

    public async Task SendDoc(int docId, object doc)
    {
        Console.WriteLine($"Sending Doc: {doc}");
        Console.WriteLine($"Sending Doc: {docId} to {Context.ConnectionId}");
        await Clients.Group(docId.ToString()).SendAsync("ReceiveDoc", doc);
    }
}

