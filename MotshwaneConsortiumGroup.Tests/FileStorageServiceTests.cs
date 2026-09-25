using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using MotshwaneConsortiumGroup.Services.FileSystem;
using Xunit;

namespace MotshwaneConsortiumGroup.Tests;

/// <summary>
/// Minimal stand-in for IWebHostEnvironment so tests don't need a running app.
/// Uses a fresh temp folder per test run as the "content root" the uploads folder is created under.
/// </summary>
public class FakeWebHostEnvironment : IWebHostEnvironment
{
    public FakeWebHostEnvironment()
    {
        ContentRootPath = Path.Combine(Path.GetTempPath(), "MotshwaneTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(ContentRootPath);
    }

    public string ApplicationName { get; set; } = "MotshwaneConsortiumGroup.Tests";
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
    public string EnvironmentName { get; set; } = "Development";
    public string WebRootPath { get; set; } = "";
    public IFileProvider WebRootFileProvider { get; set; } = null!;
}

public class FileStorageServiceTests
{
    private static LocalFileStorageService CreateService() => new(new FakeWebHostEnvironment());

    private static MemoryStream StreamOf(byte[] bytes) => new(bytes);

    private static byte[] ValidPdfBytes() =>
        System.Text.Encoding.ASCII.GetBytes("%PDF-1.4\n").Concat(new byte[100]).ToArray();

    private static byte[] ValidPngBytes() =>
        new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }.Concat(new byte[100]).ToArray();

    private static byte[] ValidJpgBytes() =>
        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }.Concat(new byte[100]).ToArray();

    [Fact]
    public async Task SaveAsync_WithAValidPdf_Succeeds()
    {
        var service = CreateService();
        await using var stream = StreamOf(ValidPdfBytes());

        var result = await service.SaveAsync(stream, "proof.pdf", "application/pdf", stream.Length);

        Assert.True(result.Success);
        Assert.EndsWith(".pdf", result.Value);
    }

    [Fact]
    public async Task SaveAsync_WithAValidPng_Succeeds()
    {
        var service = CreateService();
        await using var stream = StreamOf(ValidPngBytes());

        var result = await service.SaveAsync(stream, "proof.png", "image/png", stream.Length);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task SaveAsync_WithAValidJpg_Succeeds()
    {
        var service = CreateService();
        await using var stream = StreamOf(ValidJpgBytes());

        var result = await service.SaveAsync(stream, "proof.jpg", "image/jpeg", stream.Length);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task SaveAsync_GeneratesADifferentNameThanTheOriginal()
    {
        // The saved (server-side) file name should never be the customer-supplied name —
        // that's what stops someone guessing another customer's proof file URL.
        var service = CreateService();
        await using var stream = StreamOf(ValidPdfBytes());

        var result = await service.SaveAsync(stream, "my_original_name.pdf", "application/pdf", stream.Length);

        Assert.True(result.Success);
        Assert.NotEqual("my_original_name.pdf", result.Value);
    }

    [Fact]
    public async Task SaveAsync_WithAnUnsupportedExtension_Fails()
    {
        var service = CreateService();
        await using var stream = StreamOf(ValidPdfBytes()); // content doesn't matter, extension is checked first

        var result = await service.SaveAsync(stream, "proof.exe", "application/octet-stream", stream.Length);

        Assert.False(result.Success);
        Assert.Contains("PDF, JPG or PNG", result.Error);
    }

    [Fact]
    public async Task SaveAsync_WithAPdfExtensionButWrongContent_Fails()
    {
        // Simulates renaming another file type to .pdf to slip past an extension-only check.
        var service = CreateService();
        var fakeContent = System.Text.Encoding.ASCII.GetBytes("MZ\x90\x00this is actually an exe");
        await using var stream = StreamOf(fakeContent);

        var result = await service.SaveAsync(stream, "renamed.pdf", "application/pdf", stream.Length);

        Assert.False(result.Success);
        Assert.Contains("doesn't match", result.Error);
    }

    [Fact]
    public async Task SaveAsync_OverTheSizeLimit_Fails()
    {
        var service = CreateService();
        var oversized = System.Text.Encoding.ASCII.GetBytes("%PDF-1.4\n")
            .Concat(new byte[6 * 1024 * 1024]).ToArray(); // 6MB, over the 5MB cap
        await using var stream = StreamOf(oversized);

        var result = await service.SaveAsync(stream, "big.pdf", "application/pdf", oversized.Length);

        Assert.False(result.Success);
        Assert.Contains("too large", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveAsync_WithZeroLength_Fails()
    {
        var service = CreateService();
        await using var stream = StreamOf(Array.Empty<byte>());

        var result = await service.SaveAsync(stream, "empty.pdf", "application/pdf", 0);

        Assert.False(result.Success);
        Assert.Contains("empty", result.Error, StringComparison.OrdinalIgnoreCase);
    }
}
