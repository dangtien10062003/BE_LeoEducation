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
public class CoursesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IImageStorageService _imageStorage;

    public CoursesController(ApplicationDbContext db, IImageStorageService imageStorage)
    {
        _db = db;
        _imageStorage = imageStorage;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] CourseFilterQuery query)
    {
        var q = _db.Courses
            .Include(c => c.Subject)
            .Include(c => c.Instructor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim().ToLower();
            q = q.Where(c => c.CourseName.ToLower().Contains(keyword)
                          || (c.Description != null && c.Description.ToLower().Contains(keyword))
                          || (c.Subject != null && c.Subject.SubjectName.ToLower().Contains(keyword))
                          || (c.Instructor != null && c.Instructor.FullName.ToLower().Contains(keyword)));
        }

        if (query.SubjectId.HasValue)
            q = q.Where(c => c.SubjectId == query.SubjectId.Value);

        var total = await q.CountAsync();

        var items = await q
            .OrderByDescending(c => c.CreatedAt)
            .Skip(query.Offset)
            .Take(query.PageSize)
            .Select(c => new
            {
                c.CourseId,
                c.CourseName,
                c.Description,
                c.ImageUrl,
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
                c.BillingType,
                c.StartDate,
                c.EndDate,
                DurationMonths = TuitionCalculator.GetDurationMonths(c),
                TotalFee = TuitionCalculator.GetTotalFee(c),
                c.CreatedAt,
                c.UpdatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            message = "Thành công",
            data = items,
            pageIndex = query.PageIndex,
            pageSize = query.PageSize,
            page = query.PageIndex,
            limit = query.PageSize,
            total,
            totalPages = (int)Math.Ceiling((double)total / query.PageSize)
        });
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
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
                c.ImageUrl,
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
                c.BillingType,
                c.StartDate,
                c.EndDate,
                DurationMonths = TuitionCalculator.GetDurationMonths(c),
                TotalFee = TuitionCalculator.GetTotalFee(c),
                c.CreatedAt,
                c.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (course == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy khóa học"));

        return Ok(ApiResponse<object>.Ok(course));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateCourseRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        if (request.SubjectId.HasValue && !await _db.Subjects.AnyAsync(s => s.SubjectId == request.SubjectId.Value))
            return BadRequest(ApiResponse<object>.Fail("Môn học không tồn tại"));
        if (request.InstructorId.HasValue && !await _db.Instructors.AnyAsync(i => i.Id == request.InstructorId.Value))
            return BadRequest(ApiResponse<object>.Fail("Giáo viên không tồn tại"));

        var imageValidationError = ImageUploadValidator.GetValidationError(request.File);
        if (imageValidationError != null)
            return BadRequest(ApiResponse<object>.Fail(imageValidationError));

        var imageUrl = request.File != null && request.File.Length > 0
            ? await _imageStorage.SaveAsync(request.File, "courses", Request, HttpContext.RequestAborted)
            : request.ImageUrl;

        var course = new Course
        {
            CourseName = request.CourseName,
            Description = request.Description,
            ImageUrl = imageUrl,
            SubjectId = request.SubjectId,
            InstructorId = request.InstructorId,
            Price = request.Price,
            BillingType = string.IsNullOrWhiteSpace(request.BillingType) ? TuitionCalculator.FullCourse : request.BillingType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        course.HashCode = HashCodeGenerator.Generate(nameof(Course), course.CourseId);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { course.CourseId }, "Tạo khóa học thành công"));
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateCourseRequest request)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy khóa học"));

        if (request.SubjectId.HasValue && !await _db.Subjects.AnyAsync(s => s.SubjectId == request.SubjectId.Value))
            return BadRequest(ApiResponse<object>.Fail("Môn học không tồn tại"));
        if (request.InstructorId.HasValue && !await _db.Instructors.AnyAsync(i => i.Id == request.InstructorId.Value))
            return BadRequest(ApiResponse<object>.Fail("Giáo viên không tồn tại"));

        var imageValidationError = ImageUploadValidator.GetValidationError(request.File);
        if (imageValidationError != null)
            return BadRequest(ApiResponse<object>.Fail(imageValidationError));

        if (request.CourseName != null) course.CourseName = request.CourseName;
        if (request.Description != null) course.Description = request.Description;
        if (request.File != null && request.File.Length > 0)
            course.ImageUrl = await _imageStorage.SaveAsync(request.File, "courses", Request, HttpContext.RequestAborted);
        else if (request.ImageUrl != null)
            course.ImageUrl = request.ImageUrl;
        if (request.SubjectId.HasValue) course.SubjectId = request.SubjectId;
        if (request.InstructorId.HasValue) course.InstructorId = request.InstructorId;
        if (request.Price.HasValue) course.Price = request.Price;
        if (!string.IsNullOrWhiteSpace(request.BillingType)) course.BillingType = request.BillingType;
        course.StartDate = request.StartDate;
        course.EndDate = request.EndDate;
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
