using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;
using LeoEducation.Api.Utils;

namespace LeoEducation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstructorsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public InstructorsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ActiveFilterQuery request)
    {
        var query = _db.Instructors.AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(i => i.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(i => i.FullName.ToLower().Contains(keyword)
                                  || (i.Role != null && i.Role.ToLower().Contains(keyword))
                                  || (i.Bio != null && i.Bio.ToLower().Contains(keyword))
                                  || (i.Experience != null && i.Experience.ToLower().Contains(keyword)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(i => i.Rating)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return Ok(PagedResponse<object>.Ok(items.Cast<object>().ToList(), request.PageIndex, request.PageSize, total));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var instructor = await _db.Instructors.FindAsync(id);
        if (instructor == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy giáo viên"));

        return Ok(ApiResponse<Instructor>.Ok(instructor));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInstructorRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var instructor = new Instructor
        {
            FullName = request.FullName,
            Role = request.Role,
            Bio = request.Bio,
            AvatarUrl = request.AvatarUrl,
            Rating = request.Rating,
            Experience = request.Experience,
            IsActive = true
        };

        _db.Instructors.Add(instructor);
        await _db.SaveChangesAsync();

        instructor.HashCode = HashCodeGenerator.Generate(nameof(Instructor), instructor.Id);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Instructor>.Ok(instructor, "Thêm giáo viên thành công"));
    }

    [HttpPost("upload-avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("Vui lòng chọn ảnh"));

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(ApiResponse<object>.Fail("Ảnh không được vượt quá 5MB"));

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        if (!allowedExtensions.Contains(extension))
            return BadRequest(ApiResponse<object>.Fail("Chỉ hỗ trợ ảnh jpg, jpeg, png, webp hoặc gif"));

        var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "instructors");
        Directory.CreateDirectory(uploadRoot);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadRoot, fileName);
        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        var url = $"{Request.Scheme}://{Request.Host}/uploads/instructors/{fileName}";
        return Ok(ApiResponse<object>.Ok(new { url }, "Upload ảnh thành công"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateInstructorRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var instructor = await _db.Instructors.FindAsync(id);
        if (instructor == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy giáo viên"));

        instructor.FullName = request.FullName;
        instructor.Role = request.Role;
        instructor.Bio = request.Bio;
        instructor.AvatarUrl = request.AvatarUrl;
        instructor.Rating = request.Rating;
        instructor.Experience = request.Experience;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Instructor>.Ok(instructor, "Cập nhật giáo viên thành công"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var instructor = await _db.Instructors.FindAsync(id);
        if (instructor == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy giáo viên"));

        instructor.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { instructor.Id }, "Đã ẩn giáo viên"));
    }
}
