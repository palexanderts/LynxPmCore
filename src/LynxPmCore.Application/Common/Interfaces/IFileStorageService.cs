namespace LynxPmCore.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> StoreAsync(Stream stream, string fileName, string contentType, string folder, CancellationToken ct = default);
    Task DeleteAsync(string relativePath, CancellationToken ct = default);
}
