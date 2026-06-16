using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;
using LeoEducation.Api.Utils;

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
    /// POST /api/registrations â€” ÄÄƒng kÃ½ khÃ³a há»c
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
            return BadRequest(ApiResponse<object>.Fail("KhÃ³a há»c khÃ´ng tá»“n táº¡i hoáº·c Ä‘Ã£ Ä‘Ã³ng"));

        var registration = new CourseRegistration
        {
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            CourseId = request.CourseId,
            Status = "Má»›i",
            CreatedAt = DateTime.UtcNow
        };

        _db.CourseRegistrations.Add(registration);
        await _db.SaveChangesAsync();

        registration.HashCode = HashCodeGenerator.Generate(nameof(CourseRegistration), registration.RegistrationId);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            new { registration.RegistrationId, CourseName = course.CourseName },
            "ÄÄƒng kÃ½ thÃ nh cÃ´ng! ChÃºng tÃ´i sáº½ liÃªn há»‡ vá»›i báº¡n sá»›m."));
    }

    /// <summary>
    /// GET /api/registrations â€” Láº¥y danh sÃ¡ch Ä‘Äƒng kÃ½ (Admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery request)
    {
        var query = _db.CourseRegistrations
            .Include(r => r.Course)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(r => r.FullName.ToLower().Contains(keyword)
                                  || (r.Email != null && r.Email.ToLower().Contains(keyword))
                                  || r.Phone.ToLower().Contains(keyword)
                                  || r.Status.ToLower().Contains(keyword)
                                  || r.Course.CourseName.ToLower().Contains(keyword));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip(request.Offset)
            .Take(request.PageSize)
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

        return Ok(PagedResponse<object>.Ok(items.Cast<object>().ToList(), request.PageIndex, request.PageSize, total));
    }

    /// <summary>
    /// PATCH /api/registrations/{id} â€” Cáº­p nháº­t tráº¡ng thÃ¡i Ä‘Äƒng kÃ½
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
            return NotFound(ApiResponse<object>.Fail("KhÃ´ng tÃ¬m tháº¥y Ä‘Äƒng kÃ½"));

        registration.Status = request.Status;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            new { registration.RegistrationId, registration.Status },
            "Cáº­p nháº­t tráº¡ng thÃ¡i thÃ nh cÃ´ng"));
    }

    /// <summary>
    /// PUT /api/registrations/{id} — C?p nh?t toàn b? dang ký
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
            return NotFound(ApiResponse<object>.Fail("Không tìm th?y dang ký"));

        var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == request.CourseId);
        if (course == null)
            return BadRequest(ApiResponse<object>.Fail("Khóa h?c không t?n t?i"));

        registration.FullName = request.FullName;
        registration.Email = request.Email;
        registration.Phone = request.Phone;
        registration.CourseId = request.CourseId;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            new { registration.RegistrationId, CourseName = course.CourseName },
            "C?p nh?t dang ký thành công"));
    }

    /// <summary>
    /// DELETE /api/registrations/{id} — Xóa dang ký
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var registration = await _db.CourseRegistrations.FindAsync(id);
        if (registration == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm th?y dang ký"));

        _db.CourseRegistrations.Remove(registration);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { registration.RegistrationId }, "Ðã xóa dang ký"));
    }
}


