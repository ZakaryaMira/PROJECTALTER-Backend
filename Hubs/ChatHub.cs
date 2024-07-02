using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
namespace PROJECTALTERAPI.Hubs;

public sealed class ChatHub : Hub
{
    public async Task JoinChat(User conn)
    {
        await Clients.All.SendAsync("ReceiveMessage", "admin", $"{conn.Username} has joined the chat");
    }
     public async Task ReceiveMessage(long senderId, long receiverId, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", senderId, receiverId, message);
    }

    public async Task JoinSpecificChatRoom(User conn)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conn.Username);
        await Clients.Group(conn.Username)
        .SendAsync("ReceiveMessage", "admin", $"{conn.Username} has joined the chat");
    }

    // thats working... wallahi 
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
