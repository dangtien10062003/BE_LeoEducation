using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;
using LeoEducation.Api.Utils;

namespace LeoEducation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ContactController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContactRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var contact = new ContactRequest
        {
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Message = request.Message,
            Status = "New",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.ContactRequests.Add(contact);
        await _db.SaveChangesAsync();

        contact.HashCode = HashCodeGenerator.Generate(nameof(ContactRequest), contact.Id);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { contact.Id }, "Gửi yêu cầu thành công! Chúng tôi sẽ liên hệ với bạn sớm."));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery request)
    {
        var query = _db.ContactRequests.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(c => c.FullName.ToLower().Contains(keyword)
                                  || c.Email.ToLower().Contains(keyword)
                                  || c.Phone.ToLower().Contains(keyword)
                                  || c.Status.ToLower().Contains(keyword)
                                  || (c.Message != null && c.Message.ToLower().Contains(keyword)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .ToListAsync();

        return Ok(PagedResponse<object>.Ok(items.Cast<object>().ToList(), request.PageIndex, request.PageSize, total));
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateContactStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var contact = await _db.ContactRequests.FindAsync(id);
        if (contact == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy yêu cầu liên hệ"));

        contact.Status = request.Status;
        contact.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { contact.Id, contact.Status }, "Cập nhật trạng thái thành công"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateContactRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var contact = await _db.ContactRequests.FindAsync(id);
        if (contact == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy yêu cầu liên hệ"));

        contact.FullName = request.FullName;
        contact.Email = request.Email;
        contact.Phone = request.Phone;
        contact.Message = request.Message;
        contact.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { contact.Id }, "Cập nhật liên hệ thành công"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var contact = await _db.ContactRequests.FindAsync(id);
        if (contact == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy yêu cầu liên hệ"));

        _db.ContactRequests.Remove(contact);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { contact.Id }, "Đã xóa liên hệ"));
    }
}
