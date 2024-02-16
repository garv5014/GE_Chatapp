using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Services;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);


builder.Services
    .AddHttpClient("My.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
if (builder.HostEnvironment.IsDevelopment())
{
  builder.Services.AddSingleton(new SignalREnv()
  {
    IsServer = false,
    chatHubURL = "http://localhost:5202/ws/chatHub"
  });

}
else
{
  builder.Services.AddSingleton(new SignalREnv()
  {
    IsServer = false,
    chatHubURL = builder.HostEnvironment.BaseAddress + "ws/chatHub"
  });
}
builder.Services.AddScoped<IFileAPIService, FileApiService>(sp => new FileApiService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("My.ServerAPI")));
builder.Services.AddScoped<IChatService, ChatService>(sp => new ChatService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("My.ServerAPI"), sp.GetRequiredService<IFileAPIService>()));
await builder.Build().RunAsync();