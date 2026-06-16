using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;
using LeoEducation.Api.Utils;

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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ActiveFilterQuery request)
    {
        var query = _db.Testimonials.AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(t => t.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(t => t.StudentName.ToLower().Contains(keyword)
                                  || (t.JobTitle != null && t.JobTitle.ToLower().Contains(keyword))
                                  || t.Content.ToLower().Contains(keyword));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return Ok(PagedResponse<object>.Ok(items.Cast<object>().ToList(), request.PageIndex, request.PageSize, total));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var testimonial = await _db.Testimonials.FindAsync(id);
        if (testimonial == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy đánh giá"));

        return Ok(ApiResponse<Testimonial>.Ok(testimonial));
    }

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
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Testimonials.Add(testimonial);
        await _db.SaveChangesAsync();

        testimonial.HashCode = HashCodeGenerator.Generate(nameof(Testimonial), testimonial.TestimonialId);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Testimonial>.Ok(testimonial, "Thêm đánh giá thành công"));
    }

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
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy đánh giá"));

        testimonial.StudentName = request.StudentName;
        testimonial.JobTitle = request.JobTitle;
        testimonial.Content = request.Content;
        testimonial.Rating = request.Rating;
        testimonial.AvatarURL = request.AvatarURL;
        testimonial.IsActive = request.IsActive;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Testimonial>.Ok(testimonial, "Cập nhật đánh giá thành công"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var testimonial = await _db.Testimonials.FindAsync(id);
        if (testimonial == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy đánh giá"));

        testimonial.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { testimonial.TestimonialId }, "Đã ẩn đánh giá"));
    }
}
