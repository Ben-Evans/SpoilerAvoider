using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<TimeZoneService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<LocalCacheService>();

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });


// Register MudBlazor services for the WASM client (provides IPopoverService, snackbar, etc.)
builder.Services.AddMudServices();

/*WebAssemblyHost build = builder.Build();

IJSRuntime jsRuntime = build.Services.GetRequiredService<IJSRuntime>();
TimeZoneService timeZoneService = new(jsRuntime);
string timeZone = await timeZoneService.GetTimeZoneAsync();

builder.Services.AddScoped(sp =>
{
    HttpClient httpClient = new() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    httpClient.DefaultRequestHeaders.Add("Time-Zone", timeZone);
    return httpClient;
});

build = builder.Build();*/

// Register the handler and HttpClient
builder.Services.AddTransient<TimeZoneHandler>();
builder.Services.AddHttpClient("Default", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
})
.AddHttpMessageHandler<TimeZoneHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Default"));

await builder.Build().RunAsync();
