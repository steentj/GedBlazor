using GedBlazor.Components;
using GedBlazor.Parsers;
using GedBlazor.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<IGedcomParser, GedcomParser>();
builder.Services.AddScoped<IWordDocumentService, WordDocumentService>();
builder.Services.AddScoped<IFileDownloadService, FileDownloadService>();
builder.Services.AddRadzenComponents();

builder.RootComponents.Add<App>("#app");

await builder.Build().RunAsync();
