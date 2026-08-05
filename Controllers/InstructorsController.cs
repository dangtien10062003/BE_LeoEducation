using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;
using LeoEducation.Api.Services;
using LeoEducation.Api.Utils;

namespace LeoEducation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InstructorsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IImageStorageService _imageStorage;

    public InstructorsController(ApplicationDbContext db, IImageStorageService imageStorage)
    {
        _db = db;
        _imageStorage = imageStorage;
    }

    [HttpGet]
    [AllowAnonymous]
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
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var instructor = await _db.Instructors.FindAsync(id);
        if (instructor == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy giáo viên"));

        return Ok(ApiResponse<Instructor>.Ok(instructor));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateInstructorRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var imageValidationError = ImageUploadValidator.GetValidationError(request.File);
        if (imageValidationError != null)
            return BadRequest(ApiResponse<object>.Fail(imageValidationError));

        var avatarUrl = request.File != null && request.File.Length > 0
            ? await _imageStorage.SaveAsync(request.File, "instructors", Request, HttpContext.RequestAborted)
            : request.AvatarUrl;

        var instructor = new Instructor
        {
            FullName = request.FullName,
            Role = request.Role,
            Bio = request.Bio,
            AvatarUrl = avatarUrl,
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

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] CreateInstructorRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var instructor = await _db.Instructors.FindAsync(id);
        if (instructor == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy giáo viên"));

        var imageValidationError = ImageUploadValidator.GetValidationError(request.File);
        if (imageValidationError != null)
            return BadRequest(ApiResponse<object>.Fail(imageValidationError));

        instructor.FullName = request.FullName;
        instructor.Role = request.Role;
        instructor.Bio = request.Bio;
        instructor.AvatarUrl = request.File != null && request.File.Length > 0
            ? await _imageStorage.SaveAsync(request.File, "instructors", Request, HttpContext.RequestAborted)
            : request.AvatarUrl;
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
