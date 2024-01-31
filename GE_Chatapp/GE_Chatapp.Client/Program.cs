using Chatapp.Shared.Interfaces;
using Chatapp.Shared.Services;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);


builder.Services
    .AddHttpClient("My.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));


builder.Services.AddScoped<IFileAPIService, FileApiService>(sp => new FileApiService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("My.ServerAPI")));
builder.Services.AddScoped<IChatService, ChatService>(sp => new ChatService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("My.ServerAPI"), sp.GetRequiredService<IFileAPIService>()));

await builder.Build().RunAsync();