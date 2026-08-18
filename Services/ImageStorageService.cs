using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace LeoEducation.Api.Services;

public sealed class ImageStorageOptions
{
    public string? CloudName { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
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
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ImageStorageService> _logger;

    public ImageStorageService(
        IOptions<ImageStorageOptions> options,
        IWebHostEnvironment environment,
        IHttpClientFactory httpClientFactory,
        ILogger<ImageStorageService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> SaveAsync(IFormFile file, string folder, HttpRequest request, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var publicId = $"{Guid.NewGuid():N}{extension}";

        if (HasCloudinaryConfiguration())
            return await SaveToCloudinaryAsync(file, folder, publicId, cancellationToken);

        return await SaveToLocalDiskAsync(file, folder, publicId, request, cancellationToken);
    }

    public async Task<object> CheckHealthAsync(CancellationToken cancellationToken)
    {
        var cloudName = _options.CloudName?.Trim();
        var apiKey = _options.ApiKey?.Trim();
        var apiSecret = _options.ApiSecret?.Trim();
        var configured =
            !string.IsNullOrWhiteSpace(cloudName)
            && !string.IsNullOrWhiteSpace(apiKey)
            && !string.IsNullOrWhiteSpace(apiSecret);

        if (!HasAnyCloudinaryConfiguration())
        {
            return new
            {
                mode = "local",
                configured = false,
                message = "Cloudinary is not configured; uploads use local disk."
            };
        }

        if (!configured)
        {
            return new
            {
                mode = "cloudinary",
                configured = false,
                cloudName = !string.IsNullOrWhiteSpace(cloudName),
                apiKey = !string.IsNullOrWhiteSpace(apiKey),
                apiSecret = !string.IsNullOrWhiteSpace(apiSecret),
                message = "Cloudinary configuration is incomplete."
            };
        }

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.cloudinary.com/v1_1/{Uri.EscapeDataString(cloudName!)}/resources/image?max_results=1");
        request.Headers.Authorization = CreateBasicAuthHeader(apiKey!, apiSecret!);

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            return new
            {
                mode = "cloudinary",
                configured = true,
                reachable = response.IsSuccessStatusCode,
                cloudName,
                apiKey = Mask(apiKey!),
                statusCode = (int)response.StatusCode,
                message = response.IsSuccessStatusCode ? "Cloudinary credentials are valid." : body
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Cloudinary health check failed. CloudName={CloudName}", cloudName);
            return new
            {
                mode = "cloudinary",
                configured = true,
                reachable = false,
                cloudName,
                apiKey = Mask(apiKey!),
                message = ex.Message
            };
        }
    }

    private async Task<string> SaveToCloudinaryAsync(
        IFormFile file,
        string folder,
        string publicId,
        CancellationToken cancellationToken)
    {
        var cloudName = _options.CloudName?.Trim();
        var apiKey = _options.ApiKey?.Trim();
        var apiSecret = _options.ApiSecret?.Trim();

        if (string.IsNullOrWhiteSpace(cloudName)
            || string.IsNullOrWhiteSpace(apiKey)
            || string.IsNullOrWhiteSpace(apiSecret))
        {
            throw new InvalidOperationException("Missing Cloudinary configuration. Check Cloudinary:CloudName, Cloudinary:ApiKey, and Cloudinary:ApiSecret.");
        }

        var cloudinaryFolder = $"leo-education/{folder.Trim('/')}";
        var publicIdWithoutExtension = Path.GetFileNameWithoutExtension(publicId);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signature = CreateSignature(new Dictionary<string, string>
        {
            ["folder"] = cloudinaryFolder,
            ["public_id"] = publicIdWithoutExtension,
            ["timestamp"] = timestamp
        }, apiSecret);

        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);

        content.Add(fileContent, "file", file.FileName);
        content.Add(new StringContent(apiKey), "api_key");
        content.Add(new StringContent(timestamp), "timestamp");
        content.Add(new StringContent(signature), "signature");
        content.Add(new StringContent(cloudinaryFolder), "folder");
        content.Add(new StringContent(publicIdWithoutExtension), "public_id");

        var uploadUrl = $"https://api.cloudinary.com/v1_1/{Uri.EscapeDataString(cloudName)}/image/upload";
        _logger.LogInformation(
            "Uploading image to Cloudinary. CloudName={CloudName}, Folder={Folder}, PublicId={PublicId}, ApiKey={ApiKey}",
            cloudName,
            cloudinaryFolder,
            publicIdWithoutExtension,
            Mask(apiKey));

        var client = _httpClientFactory.CreateClient();
        using var response = await client.PostAsync(uploadUrl, content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Cloudinary upload failed. StatusCode={StatusCode}, CloudName={CloudName}, Folder={Folder}, Response={Response}",
                response.StatusCode,
                cloudName,
                cloudinaryFolder,
                body);

            throw new ImageStorageException($"Không upload được ảnh lên Cloudinary ({response.StatusCode}). Kiểm tra CloudName, API Key và API Secret.");
        }

        var result = JsonSerializer.Deserialize<CloudinaryUploadResponse>(body);
        if (string.IsNullOrWhiteSpace(result?.SecureUrl))
            throw new ImageStorageException("Cloudinary upload succeeded but did not return a secure URL.");

        return result.SecureUrl;
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

    private bool HasAnyCloudinaryConfiguration()
    {
        return !string.IsNullOrWhiteSpace(_options.CloudName)
            || !string.IsNullOrWhiteSpace(_options.ApiKey)
            || !string.IsNullOrWhiteSpace(_options.ApiSecret);
    }

    private bool HasCloudinaryConfiguration()
    {
        return !string.IsNullOrWhiteSpace(_options.CloudName)
            && !string.IsNullOrWhiteSpace(_options.ApiKey)
            && !string.IsNullOrWhiteSpace(_options.ApiSecret);
    }

    private static AuthenticationHeaderValue CreateBasicAuthHeader(string apiKey, string apiSecret)
    {
        var value = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{apiSecret}"));
        return new AuthenticationHeaderValue("Basic", value);
    }

    private static string CreateSignature(IReadOnlyDictionary<string, string> parameters, string apiSecret)
    {
        var payload = string.Join("&", parameters
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => $"{pair.Key}={pair.Value}"));

        var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(payload + apiSecret));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string Mask(string value)
    {
        if (value.Length <= 8)
            return "***";

        return $"{value[..4]}...{value[^4..]}";
    }

    private sealed class CloudinaryUploadResponse
    {
        [JsonPropertyName("secure_url")]
        public string? SecureUrl { get; set; }
    }
}
