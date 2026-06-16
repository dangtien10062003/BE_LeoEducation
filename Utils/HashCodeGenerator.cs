using System.Security.Cryptography;
using System.Text;

namespace LeoEducation.Api.Utils;

public static class HashCodeGenerator
{
    public static string Generate(string entityName, int id)
    {
        var input = $"{entityName}:{id}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
