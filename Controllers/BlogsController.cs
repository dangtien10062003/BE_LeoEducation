using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;
using LeoEducation.Api.Utils;

namespace LeoEducation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BlogsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery request)
    {
        var query = _db.Blogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(b => b.Title.ToLower().Contains(keyword)
                                  || (b.Summary != null && b.Summary.ToLower().Contains(keyword))
                                  || (b.Content != null && b.Content.ToLower().Contains(keyword))
                                  || (b.Author != null && b.Author.ToLower().Contains(keyword)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return Ok(PagedResponse<object>.Ok(items.Cast<object>().ToList(), request.PageIndex, request.PageSize, total));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var blog = await _db.Blogs.FindAsync(id);
        if (blog == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy bài viết"));

        return Ok(ApiResponse<Blog>.Ok(blog));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBlogRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var blog = new Blog
        {
            Title = request.Title,
            Summary = request.Summary,
            Content = request.Content,
            ImageUrl = request.ImageUrl,
            Author = request.Author,
            CreatedAt = DateTime.UtcNow
        };

        _db.Blogs.Add(blog);
        await _db.SaveChangesAsync();

        blog.HashCode = HashCodeGenerator.Generate(nameof(Blog), blog.Id);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Blog>.Ok(blog, "Tạo bài viết thành công"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateBlogRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var blog = await _db.Blogs.FindAsync(id);
        if (blog == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy bài viết"));

        blog.Title = request.Title;
        blog.Summary = request.Summary;
        blog.Content = request.Content;
        blog.ImageUrl = request.ImageUrl;
        blog.Author = request.Author;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Blog>.Ok(blog, "Cập nhật bài viết thành công"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var blog = await _db.Blogs.FindAsync(id);
        if (blog == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy bài viết"));

        _db.Blogs.Remove(blog);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { blog.Id }, "Đã xóa bài viết"));
    }
}
