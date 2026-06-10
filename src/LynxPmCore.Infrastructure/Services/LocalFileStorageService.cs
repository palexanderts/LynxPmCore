using LynxPmCore.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace LynxPmCore.Infrastructure.Services;

internal sealed class LocalFileStorageService(IWebHostEnvironment env) : IFileStorageService
{
    public async Task<string> StoreAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken ct = default)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var safeFolder = folder.ToUpperInvariant();

        var relativeDir = Path.Combine("uploads", "equipment", safeFolder);
        var absoluteDir = Path.Combine(env.WebRootPath, relativeDir);

        Directory.CreateDirectory(absoluteDir);

        var absolutePath = Path.Combine(absoluteDir, uniqueName);
        await using var fileStream = File.Create(absolutePath);
        await stream.CopyToAsync(fileStream, ct);

        return $"/{relativeDir.Replace('\\', '/')}/{uniqueName}";
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return Task.CompletedTask;

        var absolutePath = Path.Combine(env.WebRootPath, relativePath.TrimStart('/'));
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);

        return Task.CompletedTask;
    }
}
