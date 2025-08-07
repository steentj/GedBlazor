using GedBlazor.Components;
using GedBlazor.Parsers;
using GedBlazor.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Syncfusion.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzk4MjA0MUAzMzMwMmUzMDJlMzAzYjMzMzAzYmVGRDB0SDNyR2lFRkxjUGhsWHBhS1pVT1BiVk85VUs5MjFzQ21ocGx1bnc9");
builder.Services.AddScoped<IGedcomParser, GedcomParser>();
builder.Services.AddScoped<IWordDocumentService, WordDocumentService>();
builder.Services.AddScoped<IFileDownloadService, FileDownloadService>();
builder.Services.AddSyncfusionBlazor();

builder.RootComponents.Add<App>("#app");

await builder.Build().RunAsync();
