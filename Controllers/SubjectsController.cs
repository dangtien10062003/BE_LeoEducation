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
public class SubjectsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IImageStorageService _imageStorage;

    public SubjectsController(ApplicationDbContext db, IImageStorageService imageStorage)
    {
        _db = db;
        _imageStorage = imageStorage;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] SubjectFilterQuery request)
    {
        var query = _db.Subjects
            .Include(s => s.Courses)
            .AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(s => s.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(s => s.SubjectName.ToLower().Contains(keyword)
                                  || (s.Description != null && s.Description.ToLower().Contains(keyword)));
        }

        var total = await query.CountAsync();

        var subjects = await query
            .OrderBy(s => s.SubjectName)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .Select(s => new
            {
                s.SubjectId,
                s.SubjectName,
                s.Description,
                s.ImageUrl,
                s.IsActive,
                CourseCount = s.Courses.Count,
                s.CreatedAt,
                s.UpdatedAt
            })
            .ToListAsync();

        return Ok(PagedResponse<object>.Ok(subjects.Cast<object>().ToList(), request.PageIndex, request.PageSize, total));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var subject = await _db.Subjects
            .Include(s => s.Courses)
            .FirstOrDefaultAsync(s => s.SubjectId == id);

        if (subject == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy môn học"));

        return Ok(ApiResponse<Subject>.Ok(subject));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateSubjectRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        if (await _db.Subjects.AnyAsync(s => s.SubjectName == request.SubjectName))
            return BadRequest(ApiResponse<object>.Fail("Môn học đã tồn tại"));

        var imageValidationError = ImageUploadValidator.GetValidationError(request.File);
        if (imageValidationError != null)
            return BadRequest(ApiResponse<object>.Fail(imageValidationError));

        var imageUrl = request.File != null && request.File.Length > 0
            ? await _imageStorage.SaveAsync(request.File, "subjects", Request, HttpContext.RequestAborted)
            : request.ImageUrl;

        var subject = new Subject
        {
            SubjectName = request.SubjectName,
            Description = request.Description,
            ImageUrl = imageUrl,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Subjects.Add(subject);
        await _db.SaveChangesAsync();

        subject.HashCode = HashCodeGenerator.Generate(nameof(Subject), subject.SubjectId);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Subject>.Ok(subject, "Tạo môn học thành công"));
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateSubjectRequest request)
    {
        var subject = await _db.Subjects.FindAsync(id);
        if (subject == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy môn học"));

        var imageValidationError = ImageUploadValidator.GetValidationError(request.File);
        if (imageValidationError != null)
            return BadRequest(ApiResponse<object>.Fail(imageValidationError));

        if (!string.IsNullOrWhiteSpace(request.SubjectName))
            subject.SubjectName = request.SubjectName;
        if (request.Description != null)
            subject.Description = request.Description;
        if (request.File != null && request.File.Length > 0)
            subject.ImageUrl = await _imageStorage.SaveAsync(request.File, "subjects", Request, HttpContext.RequestAborted);
        else if (request.ImageUrl != null)
            subject.ImageUrl = request.ImageUrl;
        if (request.IsActive.HasValue)
            subject.IsActive = request.IsActive.Value;
        subject.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Subject>.Ok(subject, "Cập nhật môn học thành công"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var subject = await _db.Subjects
            .Include(s => s.Courses)
            .FirstOrDefaultAsync(s => s.SubjectId == id);
        if (subject == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy môn học"));

        if (subject.Courses.Any())
            return BadRequest(ApiResponse<object>.Fail("Không thể xóa môn học đang có khóa học"));

        _db.Subjects.Remove(subject);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { subject.SubjectId }, "Đã xóa môn học"));
    }
}
