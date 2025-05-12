using Microsoft.JSInterop;

namespace GedBlazor.Services;

/// <summary>
/// Service interface for file download operations in the browser
/// </summary>
public interface IFileDownloadService
{
    /// <summary>
    /// Downloads a file in the browser
    /// </summary>
    /// <param name="fileName">Name of the file to be downloaded</param>
    /// <param name="fileContent">Content of the file as a byte array</param>
    Task DownloadFileAsync(string fileName, byte[] fileContent);
}
