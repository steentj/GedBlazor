using GedBlazor.Components;
using GedBlazor.Parsers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<IGedcomParser, GedcomParser>();
builder.RootComponents.Add<App>("#app");

await builder.Build().RunAsync();
