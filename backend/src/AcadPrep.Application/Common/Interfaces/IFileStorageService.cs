namespace Application.Common.Interfaces;

public class FileUploadResult
{
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
}

public interface IFileStorageService
{
    /// <summary>
    /// Uploads an image to cloud storage.
    /// </summary>
    Task<FileUploadResult> UploadImageAsync(Stream fileStream, string fileName, string? folder = null);

    /// <summary>
    /// Uploads an audio file to cloud storage.
    /// </summary>
    Task<FileUploadResult> UploadAudioAsync(Stream fileStream, string fileName, string? folder = null);

    /// <summary>
    /// Deletes a file from cloud storage using its public ID.
    /// </summary>
    Task<bool> DeleteFileAsync(string publicId);
}
