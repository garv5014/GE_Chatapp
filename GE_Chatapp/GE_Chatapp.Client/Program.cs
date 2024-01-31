using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Services;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);


builder.Services
    .AddHttpClient("My.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddScoped<IFileService, FileService>();


builder.Services.AddScoped<IChatService, ChatService>(sp => new ChatService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("My.ServerAPI")));

await builder.Build().RunAsync();