using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;

namespace LeoEducation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public RegistrationsController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// POST /api/registrations — Đăng ký khóa học
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        // Verify course exists
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == request.CourseId);
        if (course == null)
            return BadRequest(ApiResponse<object>.Fail("Khóa học không tồn tại hoặc đã đóng"));

        var registration = new CourseRegistration
        {
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            CourseId = request.CourseId,
            Status = "Mới",
            CreatedAt = DateTime.UtcNow
        };

        _db.CourseRegistrations.Add(registration);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            new { registration.RegistrationId, CourseName = course.CourseName },
            "Đăng ký thành công! Chúng tôi sẽ liên hệ với bạn sớm."));
    }

    /// <summary>
    /// GET /api/registrations — Lấy danh sách đăng ký (Admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.CourseRegistrations
            .Include(r => r.Course)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.RegistrationId,
                r.FullName,
                r.Email,
                r.Phone,
                r.CourseId,
                CourseName = r.Course.CourseName,
                r.Status,
                r.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    /// <summary>
    /// PATCH /api/registrations/{id} — Cập nhật trạng thái đăng ký
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateRegistrationStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var registration = await _db.CourseRegistrations.FindAsync(id);
        if (registration == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy đăng ký"));

        registration.Status = request.Status;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            new { registration.RegistrationId, registration.Status },
            "Cập nhật trạng thái thành công"));
    }

    /// <summary>
    /// PUT /api/registrations/{id} � C?p nh?t to�n b? dang k�
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateRegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var registration = await _db.CourseRegistrations.FindAsync(id);
        if (registration == null)
            return NotFound(ApiResponse<object>.Fail("Kh�ng t�m th?y dang k�"));

        var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == request.CourseId);
        if (course == null)
            return BadRequest(ApiResponse<object>.Fail("Kh�a h?c kh�ng t?n t?i"));

        registration.FullName = request.FullName;
        registration.Email = request.Email;
        registration.Phone = request.Phone;
        registration.CourseId = request.CourseId;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            new { registration.RegistrationId, CourseName = course.CourseName },
            "C?p nh?t dang k� th�nh c�ng"));
    }

    /// <summary>
    /// DELETE /api/registrations/{id} � X�a dang k�
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var registration = await _db.CourseRegistrations.FindAsync(id);
        if (registration == null)
            return NotFound(ApiResponse<object>.Fail("Kh�ng t�m th?y dang k�"));

        _db.CourseRegistrations.Remove(registration);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { registration.RegistrationId }, "�� x�a dang k�"));
    }
}
