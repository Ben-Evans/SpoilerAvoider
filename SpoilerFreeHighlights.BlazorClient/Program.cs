using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SpoilerFreeHighlights.BlazorClient;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<TimeZoneService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<LocalCacheService>();

builder.Services.AddMudServices();

builder.Services.AddTransient<TimeZoneHandler>();
builder.Services.AddHttpClient("Default", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue("ApiBaseAddress", "https://localhost:7137"));
})
.AddHttpMessageHandler<TimeZoneHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Default"));

await builder.Build().RunAsync();
