using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace LeoEducation.Api.Services;

public sealed class ImageStorageOptions
{
    public string? AccountId { get; set; }
    public string? Bucket { get; set; }
    public string? AccessKeyId { get; set; }
    public string? SecretAccessKey { get; set; }
    public string? PublicBaseUrl { get; set; }
}

public interface IImageStorageService
{
    Task<string> SaveAsync(IFormFile file, string folder, HttpRequest request, CancellationToken cancellationToken);
}

public sealed class ImageStorageService : IImageStorageService
{
    private readonly ImageStorageOptions _options;
    private readonly IWebHostEnvironment _environment;

    public ImageStorageService(IOptions<ImageStorageOptions> options, IWebHostEnvironment environment)
    {
        _options = options.Value;
        _environment = environment;
    }

    public async Task<string> SaveAsync(IFormFile file, string folder, HttpRequest request, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";

        if (IsR2Configured())
            return await SaveToR2Async(file, folder, fileName, cancellationToken);

        return await SaveToLocalDiskAsync(file, folder, fileName, request, cancellationToken);
    }

    private async Task<string> SaveToR2Async(IFormFile file, string folder, string fileName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
            throw new InvalidOperationException("Missing R2:PublicBaseUrl. Use an R2 public bucket URL or custom domain.");

        var key = $"{folder.Trim('/')}/{fileName}";
        var endpoint = $"https://{_options.AccountId}.r2.cloudflarestorage.com";
        var config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
            RegionEndpoint = RegionEndpoint.USEast1,
        };
        using var client = new AmazonS3Client(
            new BasicAWSCredentials(_options.AccessKeyId, _options.SecretAccessKey),
            config);

        await using var stream = file.OpenReadStream();
        await client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _options.Bucket,
            Key = key,
            InputStream = stream,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            DisablePayloadSigning = true,
            DisableDefaultChecksumValidation = true,
        }, cancellationToken);

        return $"{_options.PublicBaseUrl.TrimEnd('/')}/{key}";
    }

    private async Task<string> SaveToLocalDiskAsync(IFormFile file, string folder, string fileName, HttpRequest request, CancellationToken cancellationToken)
    {
        var uploadRoot = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", folder);
        Directory.CreateDirectory(uploadRoot);

        var filePath = Path.Combine(uploadRoot, fileName);
        await using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return $"{request.Scheme}://{request.Host}/uploads/{folder}/{fileName}";
    }

    private bool IsR2Configured()
    {
        return !string.IsNullOrWhiteSpace(_options.AccountId)
            && !string.IsNullOrWhiteSpace(_options.Bucket)
            && !string.IsNullOrWhiteSpace(_options.AccessKeyId)
            && !string.IsNullOrWhiteSpace(_options.SecretAccessKey);
    }
}
