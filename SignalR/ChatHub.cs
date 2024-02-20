using Microsoft.AspNetCore.SignalR;

namespace SignalR;

public class ChatHub : Hub
{
  public async Task SendMessage(string user, string message)
  {
    await Clients.Others.SendAsync("ReceiveMessage", user, message);
  }

  public async Task StartTyping(string user)
  {
    Console.WriteLine("Start Typing");

    await Clients.Others.SendAsync("ReceiveStartTyping", user);
  }

  public async Task StopTyping(string user)
  {
    Console.WriteLine("Stop Typing");

    await Clients.Others.SendAsync("ReceiveStopTyping", user);
  }

  public async Task UpdateMessages(DateTime time)
  {
    await Clients.All.SendAsync("ReceiveUpdateMessages", time);
  }
}