using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;

namespace LeoEducation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public AuthController(ApplicationDbContext db, IConfiguration config, IWebHostEnvironment env)
    {
        _db = db;
        _config = config;
        _env = env;
    }

    /// <summary>
    /// POST /api/auth/login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Username);

        if (user == null || !string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
            return Unauthorized(ApiResponse<object>.Fail("Sai email hoặc mật khẩu"));

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(ApiResponse<object>.Fail("Sai email hoặc mật khẩu"));

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Id = user.Id,
            Username = user.Email ?? user.Phone,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = "Admin",
            Token = token
        }, "Đăng nhập thành công"));
    }

    /// <summary>
    /// POST /api/auth/register
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(ApiResponse<object>.Fail("Email đã tồn tại"));

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = "0000000000",
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            Id = user.Id,
            Username = user.Email ?? user.Phone,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = "Admin",
            Token = token
        }, "Đăng ký thành công"));
    }

    /// <summary>
    /// POST /api/auth/change-password
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<object>.Fail("Token không hợp lệ"));

        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy user"));

        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            return BadRequest(ApiResponse<object>.Fail("Mật khẩu cũ không đúng"));

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { user.Id }, "Đổi mật khẩu thành công"));
    }

    /// <summary>
    /// GET /api/auth/me
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(ApiResponse<object>.Fail("Token không hợp lệ"));

        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy user"));

        return Ok(ApiResponse<object>.Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.Phone,
            user.AvatarURL,
            user.Status,
            user.CreatedAt,
            user.LastLoginAt
        }));
    }

    /// <summary>
    /// POST /api/auth/test-login - Dev-only password verification helper.
    /// </summary>
    [HttpPost("test-login")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> TestLogin([FromBody] TestLoginRequest request)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return NotFound(ApiResponse<object>.Fail("User not found"));

        var newHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        return Ok(ApiResponse<object>.Ok(new
        {
            user.Email,
            user.FullName,
            user.Status,
            StoredHash = user.PasswordHash.Length > 20 ? user.PasswordHash[..20] + "..." : user.PasswordHash,
            TestHash = newHash[..20] + "...",
            VerifyWithStored = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)
        }));
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _config["Jwt:Key"]
            ?? string.Empty;
        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("Missing Jwt:Key configuration.");
        var jwtIssuer = _config["Jwt:Issuer"] ?? "LeoEducation";
        var jwtExpiry = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "1440");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email ?? user.Phone),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("fullName", user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtIssuer,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtExpiry),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
