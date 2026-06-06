using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;

namespace LeoEducation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CoursesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CourseFilterQuery query)
    {
        var q = _db.Courses
            .Include(c => c.Subject)
            .Include(c => c.Instructor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.ToLower();
            q = q.Where(c => c.CourseName.ToLower().Contains(keyword)
                          || (c.Description != null && c.Description.ToLower().Contains(keyword)));
        }

        if (query.SubjectId.HasValue)
            q = q.Where(c => c.SubjectId == query.SubjectId.Value);

        var total = await q.CountAsync();

        var items = await q
            .OrderByDescending(c => c.CreatedAt)
            .Skip(query.Offset)
            .Take(query.Limit)
            .Select(c => new
            {
                c.CourseId,
                c.CourseName,
                c.Description,
                c.SubjectId,
                Subject = c.Subject == null ? null : new
                {
                    c.Subject.SubjectId,
                    c.Subject.SubjectName,
                    c.Subject.Description,
                    c.Subject.ImageUrl,
                    c.Subject.IsActive
                },
                c.InstructorId,
                Instructor = c.Instructor == null ? null : new
                {
                    c.Instructor.Id,
                    c.Instructor.FullName,
                    c.Instructor.Role,
                    c.Instructor.Bio,
                    c.Instructor.AvatarUrl,
                    c.Instructor.Rating,
                    c.Instructor.Experience,
                    c.Instructor.IsActive
                },
                c.Price,
                c.CreatedAt,
                c.UpdatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            message = "Thành công",
            data = items,
            page = query.Page,
            limit = query.Limit,
            total,
            totalPages = (int)Math.Ceiling((double)total / query.Limit)
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var course = await _db.Courses
            .Include(c => c.Subject)
            .Include(c => c.Instructor)
            .Where(c => c.CourseId == id)
            .Select(c => new
            {
                c.CourseId,
                c.CourseName,
                c.Description,
                c.SubjectId,
                Subject = c.Subject == null ? null : new
                {
                    c.Subject.SubjectId,
                    c.Subject.SubjectName,
                    c.Subject.Description,
                    c.Subject.ImageUrl,
                    c.Subject.IsActive
                },
                c.InstructorId,
                Instructor = c.Instructor == null ? null : new
                {
                    c.Instructor.Id,
                    c.Instructor.FullName,
                    c.Instructor.Role,
                    c.Instructor.Bio,
                    c.Instructor.AvatarUrl,
                    c.Instructor.Rating,
                    c.Instructor.Experience,
                    c.Instructor.IsActive
                },
                c.Price,
                c.CreatedAt,
                c.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (course == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy khóa học"));

        return Ok(ApiResponse<object>.Ok(course));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        if (request.SubjectId.HasValue && !await _db.Subjects.AnyAsync(s => s.SubjectId == request.SubjectId.Value))
            return BadRequest(ApiResponse<object>.Fail("Môn học không tồn tại"));

        var course = new Course
        {
            CourseName = request.CourseName,
            Description = request.Description,
            SubjectId = request.SubjectId,
            InstructorId = request.InstructorId,
            Price = request.Price,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { course.CourseId }, "Tạo khóa học thành công"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseRequest request)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy khóa học"));

        if (request.SubjectId.HasValue && !await _db.Subjects.AnyAsync(s => s.SubjectId == request.SubjectId.Value))
            return BadRequest(ApiResponse<object>.Fail("Môn học không tồn tại"));

        if (request.CourseName != null) course.CourseName = request.CourseName;
        if (request.Description != null) course.Description = request.Description;
        if (request.SubjectId.HasValue) course.SubjectId = request.SubjectId;
        if (request.InstructorId.HasValue) course.InstructorId = request.InstructorId;
        if (request.Price.HasValue) course.Price = request.Price;
        course.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { course.CourseId }, "Cập nhật khóa học thành công"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy khóa học"));

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { course.CourseId }, "Đã xóa khóa học"));
    }
}
