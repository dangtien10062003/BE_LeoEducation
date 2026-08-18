using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace LeoEducation.Api.Services;

public sealed class ImageStorageOptions
{
    public string? AccountId { get; set; }
    public string? Endpoint { get; set; }
    public string? Bucket { get; set; }
    public string? AccessKeyId { get; set; }
    public string? SecretAccessKey { get; set; }
    public string? PublicBaseUrl { get; set; }
}

public interface IImageStorageService
{
    Task<string> SaveAsync(IFormFile file, string folder, HttpRequest request, CancellationToken cancellationToken);
    Task<object> CheckHealthAsync(CancellationToken cancellationToken);
}

public sealed class ImageStorageException : Exception
{
    public ImageStorageException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
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

        if (HasAnyR2Configuration())
            return await SaveToR2Async(file, folder, fileName, cancellationToken);

        return await SaveToLocalDiskAsync(file, folder, fileName, request, cancellationToken);
    }

    public async Task<object> CheckHealthAsync(CancellationToken cancellationToken)
    {
        var accountId = _options.AccountId?.Trim();
        var endpoint = NormalizeEndpoint(_options.Endpoint, accountId);
        var bucket = _options.Bucket?.Trim();
        var accessKeyId = _options.AccessKeyId?.Trim();
        var secretAccessKey = _options.SecretAccessKey?.Trim();
        var publicBaseUrl = _options.PublicBaseUrl?.Trim();
        var hasAnyR2Configuration = HasAnyR2Configuration();
        var hasRequiredR2Configuration =
            !string.IsNullOrWhiteSpace(endpoint)
            && !string.IsNullOrWhiteSpace(bucket)
            && !string.IsNullOrWhiteSpace(accessKeyId)
            && !string.IsNullOrWhiteSpace(secretAccessKey)
            && !string.IsNullOrWhiteSpace(publicBaseUrl);

        if (!hasAnyR2Configuration)
        {
            return new
            {
                mode = "local",
                configured = false,
                message = "R2 is not configured; uploads use local disk."
            };
        }

        if (!hasRequiredR2Configuration)
        {
            return new
            {
                mode = "r2",
                configured = false,
                accountId = !string.IsNullOrWhiteSpace(accountId),
                endpoint = !string.IsNullOrWhiteSpace(endpoint),
                bucket = !string.IsNullOrWhiteSpace(bucket),
                accessKeyId = !string.IsNullOrWhiteSpace(accessKeyId),
                secretAccessKey = !string.IsNullOrWhiteSpace(secretAccessKey),
                publicBaseUrl = !string.IsNullOrWhiteSpace(publicBaseUrl),
                message = "R2 configuration is incomplete."
            };
        }

        var config = CreateR2Config(endpoint);
        using var client = new AmazonS3Client(
            new BasicAWSCredentials(accessKeyId, secretAccessKey),
            config);

        try
        {
            var response = await client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = bucket,
                MaxKeys = 1,
            }, cancellationToken);

            return new
            {
                mode = "r2",
                configured = true,
                reachable = true,
                endpoint,
                bucket,
                accessKeyId = Mask(accessKeyId!),
                publicBaseUrl,
                objectCountProbe = response.S3Objects.Count,
            };
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Cloudflare R2 health check failed. StatusCode={StatusCode}, ErrorCode={ErrorCode}, RequestId={RequestId}, Bucket={Bucket}", ex.StatusCode, ex.ErrorCode, ex.RequestId, bucket);
            return new
            {
                mode = "r2",
                configured = true,
                reachable = false,
                endpoint,
                bucket,
                accessKeyId = Mask(accessKeyId!),
                publicBaseUrl,
                statusCode = ex.StatusCode.ToString(),
                errorCode = ex.ErrorCode,
                requestId = ex.RequestId,
                message = ex.Message,
            };
        }
    }

    private async Task<string> SaveToR2Async(IFormFile file, string folder, string fileName, CancellationToken cancellationToken)
    {
        var accountId = _options.AccountId?.Trim();
        var endpoint = NormalizeEndpoint(_options.Endpoint, accountId);
        var bucket = _options.Bucket?.Trim();
        var accessKeyId = _options.AccessKeyId?.Trim();
        var secretAccessKey = _options.SecretAccessKey?.Trim();
        var publicBaseUrl = _options.PublicBaseUrl?.Trim();

        if (string.IsNullOrWhiteSpace(endpoint)
            || string.IsNullOrWhiteSpace(bucket)
            || string.IsNullOrWhiteSpace(accessKeyId)
            || string.IsNullOrWhiteSpace(secretAccessKey)
            || string.IsNullOrWhiteSpace(publicBaseUrl))
        {
            throw new InvalidOperationException("Missing R2 configuration. Check R2:Endpoint, R2:Bucket, R2:AccessKeyId, R2:SecretAccessKey, and R2:PublicBaseUrl.");
        }

        var key = $"{folder.Trim('/')}/{fileName}";
        _logger.LogInformation(
            "Uploading image to R2. Endpoint={Endpoint}, Bucket={Bucket}, Key={Key}, AccessKeyId={AccessKeyId}, PublicBaseUrl={PublicBaseUrl}",
            endpoint,
            bucket,
            key,
            Mask(accessKeyId),
            publicBaseUrl);

        var config = CreateR2Config(endpoint);
        using var client = new AmazonS3Client(
            new BasicAWSCredentials(accessKeyId, secretAccessKey),
            config);

        try
        {
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
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(
                ex,
                "Cloudflare R2 upload failed. StatusCode={StatusCode}, ErrorCode={ErrorCode}, RequestId={RequestId}, Bucket={Bucket}, Key={Key}",
                ex.StatusCode,
                ex.ErrorCode,
                ex.RequestId,
                bucket,
                key);

            throw new ImageStorageException(
                $"Không upload được ảnh lên Cloudflare R2 ({ex.StatusCode}, {ex.ErrorCode}). Kiểm tra bucket, AccountId, Access Key quyền Object Write và PublicBaseUrl.",
                ex);
        }
        catch (AmazonServiceException ex)
        {
            _logger.LogError(ex, "Cloudflare R2 service error while uploading image. Bucket={Bucket}, Key={Key}", bucket, key);
            throw new ImageStorageException("Không upload được ảnh lên Cloudflare R2. Kiểm tra cấu hình R2 trên Render.", ex);
        }
        catch (AmazonClientException ex)
        {
            _logger.LogError(ex, "Cloudflare R2 client error while uploading image. Bucket={Bucket}, Key={Key}", bucket, key);
            throw new ImageStorageException("Không kết nối được Cloudflare R2. Kiểm tra AccountId và network từ Render.", ex);
        }

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

    private bool HasAnyR2Configuration()
    {
        return !string.IsNullOrWhiteSpace(_options.AccountId)
            || !string.IsNullOrWhiteSpace(_options.Endpoint)
            || !string.IsNullOrWhiteSpace(_options.Bucket)
            || !string.IsNullOrWhiteSpace(_options.AccessKeyId)
            || !string.IsNullOrWhiteSpace(_options.SecretAccessKey)
            || !string.IsNullOrWhiteSpace(_options.PublicBaseUrl);
    }

    private static string NormalizeEndpoint(string? endpoint, string? accountId)
    {
        var value = endpoint?.Trim().TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(value))
            return value;

        return string.IsNullOrWhiteSpace(accountId)
            ? string.Empty
            : $"https://{accountId}.r2.cloudflarestorage.com";
    }

    private static AmazonS3Config CreateR2Config(string endpoint)
    {
        return new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
            AuthenticationRegion = "us-east-1",
            AuthenticationServiceName = "s3",
            RegionEndpoint = RegionEndpoint.USEast1,
        };
    }

    private static string Mask(string value)
    {
        if (value.Length <= 8)
            return "***";

        return $"{value[..4]}...{value[^4..]}";
    }
}
