using MotshwaneConsortiumGroup.Models;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Services.FileSystem;

/// <summary>
/// Saves files to App_Data/uploads, outside wwwroot, so they can't be browsed to directly by URL.
/// Files must be served through a controller action that checks the requester is allowed to see them.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private const long MaxBytes = 5 * 1024 * 1024; // 5MB

    // extension -> the real file signature ("magic bytes") it must start with.
    // This stops someone renaming another file type to .pdf/.jpg/.png to get past the extension check.
    private static readonly Dictionary<string, byte[][]> Signatures = new()
    {
        [".pdf"] = new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } },                         // %PDF
        [".png"] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } }, // \x89PNG....
        [".jpg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
        [".jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
    };

    private readonly string _uploadRoot;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        // ContentRootPath, not WebRootPath (wwwroot) — keeps uploads outside the publicly served folder.
        _uploadRoot = Path.Combine(env.ContentRootPath, "App_Data", "uploads");
        Directory.CreateDirectory(_uploadRoot);
    }

    public async Task<OperationResult<string>> SaveAsync(Stream content, string originalFileName, string contentType, long length)
    {
        if (length <= 0)
            return OperationResult<string>.Fail("The selected file is empty.");

        if (length > MaxBytes)
            return OperationResult<string>.Fail("File is too large. Maximum size is 5MB.");

        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!Signatures.TryGetValue(ext, out var validSignatures))
            return OperationResult<string>.Fail("Only PDF, JPG or PNG files are allowed.");

        var header = new byte[8];
        int read = await content.ReadAsync(header, 0, header.Length);
        content.Position = 0; // rewind so the caller/save can read the whole file again

        bool matchesSignature = validSignatures.Any(sig =>
            read >= sig.Length && header.Take(sig.Length).SequenceEqual(sig));

        if (!matchesSignature)
            return OperationResult<string>.Fail("The file's content doesn't match a PDF, JPG or PNG — it may be mislabelled or corrupted.");

        var savedName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(_uploadRoot, savedName);

        await using var fileStream = new FileStream(fullPath, FileMode.Create);
        await content.CopyToAsync(fileStream);

        return OperationResult<string>.Ok(savedName);
    }
}
