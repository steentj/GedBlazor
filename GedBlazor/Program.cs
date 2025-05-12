using System.Diagnostics;
using GedBlazor.Components;
using GedBlazor.Parsers;
using GedBlazor.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<IGedcomParser, GedcomParser>();
builder.Services.AddScoped<WordDocumentService>();
builder.Services.AddScoped<FileDownloadService>();
builder.RootComponents.Add<App>("#app");

Debug.Print("Debugging Blazor WebAssembly app", "GedBlazor", "Program.cs", 1);

await builder.Build().RunAsync();
