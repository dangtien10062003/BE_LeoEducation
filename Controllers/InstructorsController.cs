using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;

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

    /// <summary>
    /// GET /api/instructors â€” Láº¥y danh sÃ¡ch giÃ¡o viÃªn
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Instructors
            .Where(i => i.IsActive)
            .OrderByDescending(i => i.Rating)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    /// <summary>
    /// GET /api/instructors/{id} â€” Chi tiáº¿t giÃ¡o viÃªn
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var instructor = await _db.Instructors.FindAsync(id);
        if (instructor == null)
            return NotFound(ApiResponse<object>.Fail("KhÃ´ng tÃ¬m tháº¥y giÃ¡o viÃªn"));

        return Ok(ApiResponse<Instructor>.Ok(instructor));
    }

    /// <summary>
    /// POST /api/instructors â€” ThÃªm giÃ¡o viÃªn má»›i
    /// </summary>
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

        return Ok(ApiResponse<Instructor>.Ok(instructor, "ThÃªm giÃ¡o viÃªn thÃ nh cÃ´ng"));
    }

    /// <summary>
    /// PUT /api/instructors/{id} — C?p nh?t giáo viên
    /// </summary>
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
            return NotFound(ApiResponse<object>.Fail("Không tìm th?y giáo viên"));

        instructor.FullName = request.FullName;
        instructor.Role = request.Role;
        instructor.Bio = request.Bio;
        instructor.AvatarUrl = request.AvatarUrl;
        instructor.Rating = request.Rating;
        instructor.Experience = request.Experience;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Instructor>.Ok(instructor, "C?p nh?t giáo viên thành công"));
    }

    /// <summary>
    /// DELETE /api/instructors/{id} — Xóa giáo viên
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var instructor = await _db.Instructors.FindAsync(id);
        if (instructor == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm th?y giáo viên"));

        // Soft delete
        instructor.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { instructor.Id }, "Ðã ?n giáo viên"));
    }
}
