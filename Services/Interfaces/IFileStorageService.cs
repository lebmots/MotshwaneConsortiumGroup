namespace MotshwaneConsortiumGroup.Services.Interfaces;

using MotshwaneConsortiumGroup.Models;

/// <summary>
/// Validates and saves uploaded files (proof of payment) outside wwwroot, under a generated name.
/// LocalFileStorageService is the placeholder for now; swap for Firebase Storage behind this
/// same interface when hosting moves there — nothing above this layer needs to change.
/// </summary>
public interface IFileStorageService
{
    /// <summary>Validates extension, declared content type, real file signature and size, then saves
    /// the file under a new GUID-based name. Returns the saved (server-side) file name on success.</summary>
    Task<OperationResult<string>> SaveAsync(Stream content, string originalFileName, string contentType, long length);
}
