namespace LeoEducation.Api.Services;

public static class ImageUploadValidator
{
    private const long MaxImageSize = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif"
    };

    public static string? GetValidationError(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return null;

        if (file.Length > MaxImageSize)
            return "Ảnh không được vượt quá 5MB";

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            return "Chỉ hỗ trợ ảnh jpg, jpeg, png, webp hoặc gif";

        return null;
    }
}
