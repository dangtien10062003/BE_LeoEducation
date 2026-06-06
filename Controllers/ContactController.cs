using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;

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

    /// <summary>
    /// POST /api/contact — Gửi yêu cầu tư vấn
    /// </summary>
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

        return Ok(ApiResponse<object>.Ok(new { contact.Id }, "Gửi yêu cầu thành công! Chúng tôi sẽ liên hệ với bạn sớm."));
    }

    /// <summary>
    /// GET /api/contact — Lấy danh sách yêu cầu (Admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.ContactRequests
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    /// <summary>
    /// PATCH /api/contact/{id} — Cập nhật trạng thái
    /// </summary>
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

    /// <summary>
    /// PUT /api/contact/{id} � C?p nh?t li�n h?
    /// </summary>
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
            return NotFound(ApiResponse<object>.Fail("Kh�ng t�m th?y y�u c?u li�n h?"));

        contact.FullName = request.FullName;
        contact.Email = request.Email;
        contact.Phone = request.Phone;
        contact.Message = request.Message;
        contact.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { contact.Id }, "C?p nh?t li�n h? th�nh c�ng"));
    }

    /// <summary>
    /// DELETE /api/contact/{id} � X�a li�n h?
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var contact = await _db.ContactRequests.FindAsync(id);
        if (contact == null)
            return NotFound(ApiResponse<object>.Fail("Kh�ng t�m th?y y�u c?u li�n h?"));

        _db.ContactRequests.Remove(contact);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { contact.Id }, "�� x�a li�n h?"));
    }
}
