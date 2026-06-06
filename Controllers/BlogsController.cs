using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;

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

    /// <summary>
    /// GET /api/blogs ‚Äî L·∫•y danh s√°ch b√†i vi·∫øt
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Blogs
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    /// <summary>
    /// GET /api/blogs/{id} ‚Äî Chi ti·∫øt b√†i vi·∫øt
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var blog = await _db.Blogs.FindAsync(id);
        if (blog == null)
            return NotFound(ApiResponse<object>.Fail("Kh√¥ng t√¨m th·∫•y b√†i vi·∫øt"));

        return Ok(ApiResponse<Blog>.Ok(blog));
    }

    /// <summary>
    /// POST /api/blogs ‚Äî T·∫°o b√†i vi·∫øt m·ªõi
    /// </summary>
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

        return Ok(ApiResponse<Blog>.Ok(blog, "T·∫°o b√†i vi·∫øt th√†nh c√¥ng"));
    }

    /// <summary>
    /// PUT /api/blogs/{id} ó C?p nh?t b‡i vi?t
    /// </summary>
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
            return NotFound(ApiResponse<object>.Fail("KhÙng tÏm th?y b‡i vi?t"));

        blog.Title = request.Title;
        blog.Summary = request.Summary;
        blog.Content = request.Content;
        blog.ImageUrl = request.ImageUrl;
        blog.Author = request.Author;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Blog>.Ok(blog, "C?p nh?t b‡i vi?t th‡nh cÙng"));
    }

    /// <summary>
    /// DELETE /api/blogs/{id} ó XÛa b‡i vi?t
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var blog = await _db.Blogs.FindAsync(id);
        if (blog == null)
            return NotFound(ApiResponse<object>.Fail("KhÙng tÏm th?y b‡i vi?t"));

        _db.Blogs.Remove(blog);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { blog.Id }, "–„ xÛa b‡i vi?t"));
    }
}
