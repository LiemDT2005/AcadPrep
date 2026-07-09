using Application.Common.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class CloudinaryStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryStorageService> _logger;

    public CloudinaryStorageService(IConfiguration configuration, ILogger<CloudinaryStorageService> logger)
    {
        _logger = logger;

        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
        {
            throw new InvalidOperationException(
                "Cloudinary credentials not configuration yet. Add Cloudinary:CloudName, Cloudinary:ApiKey, and Cloudinary:ApiSecret into appsettings.Development.json.");
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true; // Luôn dùng HTTPS
    }

    public async Task<FileUploadResult> UploadImageAsync(Stream fileStream, string fileName, string? folder = null)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder ?? "acadprep/images",
            Transformation = new Transformation()
                .Quality("auto")     // Tự động tối ưu chất lượng
                .FetchFormat("auto") // Tự động chọn format tốt nhất (webp, avif...)
        };

        return await UploadAsync(uploadParams);
    }

    public async Task<FileUploadResult> UploadAudioAsync(Stream fileStream, string fileName, string? folder = null)
    {
        // Audio dùng RawUploadParams hoặc VideoUploadParams (Cloudinary xử lý audio như resource type = video)
        // Ta dùng VideoUploadParams của Cloudinary SDK để upload file audio
        var uploadParams = new VideoUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder ?? "acadprep/audio",
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            _logger.LogError("Cloudinary fail to upload audio: {Error}", uploadResult.Error.Message);
            throw new Exception($"Upload audio failed: {uploadResult.Error.Message}");
        }

        _logger.LogInformation("Upload audio successdul: {Url}", uploadResult.SecureUrl);

        return new FileUploadResult
        {
            Url = uploadResult.SecureUrl.ToString(),
            PublicId = uploadResult.PublicId
        };
    }

    public async Task<bool> DeleteFileAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Result == "ok")
        {
            _logger.LogInformation("Delete file successful: {PublicId}", publicId);
            return true;
        }

        _logger.LogWarning("Delete file failed: {PublicId} - {Result}", publicId, result.Result);
        return false;
    }

    private async Task<FileUploadResult> UploadAsync(ImageUploadParams uploadParams)
    {
        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error.Message);
            throw new Exception($"Upload failed: {uploadResult.Error.Message}");
        }

        _logger.LogInformation("Upload successful: {Url}", uploadResult.SecureUrl);

        return new FileUploadResult
        {
            Url = uploadResult.SecureUrl.ToString(),
            PublicId = uploadResult.PublicId
        };
    }
}
