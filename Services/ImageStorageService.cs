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
    private readonly ILogger<ImageStorageService> _logger;

    public ImageStorageService(
        IOptions<ImageStorageOptions> options,
        IWebHostEnvironment environment,
        ILogger<ImageStorageService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _logger = logger;
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
        var accountId = _options.AccountId?.Trim();
        var bucket = _options.Bucket?.Trim();
        var accessKeyId = _options.AccessKeyId?.Trim();
        var secretAccessKey = _options.SecretAccessKey?.Trim();
        var publicBaseUrl = _options.PublicBaseUrl?.Trim();

        if (string.IsNullOrWhiteSpace(publicBaseUrl))
            throw new InvalidOperationException("Missing R2:PublicBaseUrl. Use an R2 public bucket URL or custom domain.");

        if (string.IsNullOrWhiteSpace(accountId)
            || string.IsNullOrWhiteSpace(bucket)
            || string.IsNullOrWhiteSpace(accessKeyId)
            || string.IsNullOrWhiteSpace(secretAccessKey))
        {
            throw new InvalidOperationException("Missing R2 configuration. Check R2:AccountId, R2:Bucket, R2:AccessKeyId, and R2:SecretAccessKey.");
        }

        var key = $"{folder.Trim('/')}/{fileName}";
        var endpoint = $"https://{accountId}.r2.cloudflarestorage.com";
        _logger.LogInformation(
            "Uploading image to R2. Endpoint={Endpoint}, Bucket={Bucket}, Key={Key}, AccessKeyId={AccessKeyId}, PublicBaseUrl={PublicBaseUrl}",
            endpoint,
            bucket,
            key,
            Mask(accessKeyId),
            publicBaseUrl);

        var config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
            RegionEndpoint = RegionEndpoint.USEast1,
        };
        using var client = new AmazonS3Client(
            new BasicAWSCredentials(accessKeyId, secretAccessKey),
            config);

        await using var stream = file.OpenReadStream();
        await client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = bucket,
            Key = key,
            InputStream = stream,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            DisablePayloadSigning = true,
            DisableDefaultChecksumValidation = true,
        }, cancellationToken);

        return $"{publicBaseUrl.TrimEnd('/')}/{key}";
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

    private static string Mask(string value)
    {
        if (value.Length <= 8)
            return "***";

        return $"{value[..4]}...{value[^4..]}";
    }
}
