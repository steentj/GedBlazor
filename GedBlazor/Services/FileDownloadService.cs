using Microsoft.JSInterop;

namespace GedBlazor.Services;

/// <summary>
/// Service for file download operations in the browser using JS Interop
/// </summary>
public class FileDownloadService
{
    private readonly IJSRuntime _jsRuntime;

    public FileDownloadService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Downloads a file in the browser
    /// </summary>
    /// <param name="fileName">Name of the file to be downloaded</param>
    /// <param name="fileContent">Content of the file as a byte array</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task DownloadFileAsync(string fileName, byte[] fileContent)
    {
        // Convert the byte array to a base64 string
        var base64 = Convert.ToBase64String(fileContent);
        
        // Call JavaScript function to download the file
        await _jsRuntime.InvokeVoidAsync("downloadFileFromBase64", fileName, base64);
    }
}
