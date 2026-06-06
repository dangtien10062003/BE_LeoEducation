using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;

namespace LeoEducation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public SubjectsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var query = _db.Subjects
            .Include(s => s.Courses)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(s => s.IsActive);

        var subjects = await query
            .OrderBy(s => s.SubjectName)
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

        return Ok(ApiResponse<object>.Ok(subjects));
    }

    [HttpGet("{id}")]
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
    public async Task<IActionResult> Create([FromBody] CreateSubjectRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        if (await _db.Subjects.AnyAsync(s => s.SubjectName == request.SubjectName))
            return BadRequest(ApiResponse<object>.Fail("Môn học đã tồn tại"));

        var subject = new Subject
        {
            SubjectName = request.SubjectName,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Subjects.Add(subject);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Subject>.Ok(subject, "Tạo môn học thành công"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubjectRequest request)
    {
        var subject = await _db.Subjects.FindAsync(id);
        if (subject == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy môn học"));

        if (!string.IsNullOrWhiteSpace(request.SubjectName))
            subject.SubjectName = request.SubjectName;
        if (request.Description != null)
            subject.Description = request.Description;
        if (request.ImageUrl != null)
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
