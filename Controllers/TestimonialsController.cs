using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;

namespace LeoEducation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestimonialsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public TestimonialsController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// GET /api/testimonials â€” Láº¥y danh sÃ¡ch Ä‘Ã¡nh giÃ¡
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Testimonials
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    /// <summary>
    /// GET /api/testimonials/{id} â€” Chi tiáº¿t Ä‘Ã¡nh giÃ¡
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var testimonial = await _db.Testimonials.FindAsync(id);
        if (testimonial == null)
            return NotFound(ApiResponse<object>.Fail("KhÃ´ng tÃ¬m tháº¥y Ä‘Ã¡nh giÃ¡"));

        return Ok(ApiResponse<Testimonial>.Ok(testimonial));
    }

    /// <summary>
    /// POST /api/testimonials â€” ThÃªm Ä‘Ã¡nh giÃ¡ má»›i
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTestimonialRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var testimonial = new Testimonial
        {
            StudentName = request.StudentName,
            JobTitle = request.JobTitle,
            Content = request.Content,
            Rating = request.Rating,
            AvatarURL = request.AvatarURL,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Testimonials.Add(testimonial);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Testimonial>.Ok(testimonial, "ThÃªm Ä‘Ã¡nh giÃ¡ thÃ nh cÃ´ng"));
    }

    /// <summary>
    /// PUT /api/testimonials/{id} — C?p nh?t dánh giá
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateTestimonialRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var testimonial = await _db.Testimonials.FindAsync(id);
        if (testimonial == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm th?y dánh giá"));

        testimonial.StudentName = request.StudentName;
        testimonial.JobTitle = request.JobTitle;
        testimonial.Content = request.Content;
        testimonial.Rating = request.Rating;
        testimonial.AvatarURL = request.AvatarURL;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Testimonial>.Ok(testimonial, "C?p nh?t dánh giá thành công"));
    }

    /// <summary>
    /// DELETE /api/testimonials/{id} — Xóa dánh giá
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var testimonial = await _db.Testimonials.FindAsync(id);
        if (testimonial == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm th?y dánh giá"));

        // Soft delete
        testimonial.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { testimonial.TestimonialId }, "Ðã ?n dánh giá"));
    }
}
