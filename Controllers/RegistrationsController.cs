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
    /// POST /api/registrations - Đăng ký khóa học
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

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
            Source = string.IsNullOrWhiteSpace(request.Source) ? "Website" : request.Source.Trim(),
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
        };

        _db.CourseRegistrations.Add(registration);
        await _db.SaveChangesAsync();

        registration.HashCode = HashCodeGenerator.Generate(nameof(CourseRegistration), registration.RegistrationId);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            new { registration.RegistrationId, CourseName = course.CourseName },
            "Đăng ký thành công! Chúng tôi sẽ liên hệ với bạn sớm."));
    }

    /// <summary>
    /// GET /api/registrations - Lấy danh sách đăng ký (Admin)
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
            var matchesAssignedStatus = "đã có lớp".Contains(keyword);
            var matchesUnassignedStatus = "chưa có lớp".Contains(keyword);
            query = query.Where(r => r.FullName.ToLower().Contains(keyword)
                                  || (r.Email != null && r.Email.ToLower().Contains(keyword))
                                  || r.Phone.ToLower().Contains(keyword)
                                  || (r.Note != null && r.Note.ToLower().Contains(keyword))
                                  || r.Source.ToLower().Contains(keyword)
                                  || r.Status.ToLower().Contains(keyword)
                                  || r.Course.CourseName.ToLower().Contains(keyword)
                                  || (matchesAssignedStatus && r.ClassStudents.Any())
                                  || (matchesUnassignedStatus && !r.ClassStudents.Any()));
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
                r.StudentId,
                CourseName = r.Course.CourseName,
                CourseBillingType = r.Course.BillingType,
                CourseStartDate = r.Course.StartDate,
                CourseEndDate = r.Course.EndDate,
                DurationMonths = TuitionCalculator.GetDurationMonths(r.Course),
                TotalFee = TuitionCalculator.GetTotalFee(r.Course),
                r.Status,
                r.Source,
                r.Note,
                PaymentMode = string.IsNullOrWhiteSpace(r.PaymentMode) ? r.Course.BillingType : r.PaymentMode,
                r.PaidAmount,
                RemainingAmount = Math.Max(0, TuitionCalculator.GetTotalFee(r.Course) - r.PaidAmount),
                TuitionStatus = TuitionCalculator.GetTuitionStatus(r),
                NextTuitionDueDate = TuitionCalculator.GetNextDueDate(r),
                IsTuitionDue = TuitionCalculator.IsTuitionDue(r),
                TuitionDueStatus = TuitionCalculator.GetDueStatus(r),
                r.LastPaymentAt,
                r.TuitionNote,
                ConsultationCount = r.ConsultationLogs.Count,
                StudentStatus = r.ClassStudents.Any() ? "Đã có lớp" : "Chưa có lớp",
                AssignedClasses = r.ClassStudents
                    .OrderBy(cs => cs.Class.ClassName)
                    .Select(cs => new
                    {
                        cs.ClassId,
                        cs.Class.ClassName
                    })
                    .ToList(),
                r.CreatedAt
            })
            .ToListAsync();

        return Ok(PagedResponse<object>.Ok(items.Cast<object>().ToList(), request.PageIndex, request.PageSize, total));
    }

    /// <summary>
    /// PATCH /api/registrations/{id} - Cập nhật trạng thái đăng ký
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
    /// PUT /api/registrations/{id} - Cập nhật toàn bộ đăng ký
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
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy đăng ký"));

        var course = await _db.Courses.FirstOrDefaultAsync(c => c.CourseId == request.CourseId);
        if (course == null)
            return BadRequest(ApiResponse<object>.Fail("Khóa học không tồn tại"));

        registration.FullName = request.FullName;
        registration.Email = request.Email;
        registration.Phone = request.Phone;
        registration.CourseId = request.CourseId;
        registration.Source = string.IsNullOrWhiteSpace(request.Source) ? "Website" : request.Source.Trim();
        registration.Note = request.Note;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(
            new { registration.RegistrationId, CourseName = course.CourseName },
            "Cập nhật đăng ký thành công"));
    }

    [HttpPatch("{id}/tuition")]
    public async Task<IActionResult> UpdateTuition(int id, [FromBody] UpdateTuitionRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var registration = await _db.CourseRegistrations
            .Include(r => r.Course)
            .FirstOrDefaultAsync(r => r.RegistrationId == id);
        if (registration == null)
            return NotFound(ApiResponse<object>.Fail("KhÃ´ng tÃ¬m tháº¥y Ä‘Äƒng kÃ½"));

        registration.PaymentMode = string.IsNullOrWhiteSpace(request.PaymentMode)
            ? registration.Course.BillingType
            : request.PaymentMode;
        registration.PaidAmount = request.PaidAmount;
        registration.LastPaymentAt = request.LastPaymentAt ?? (request.PaidAmount > 0 ? DateTime.UtcNow : null);
        registration.TuitionNote = request.TuitionNote;

        await _db.SaveChangesAsync();

        var totalFee = TuitionCalculator.GetTotalFee(registration.Course);
        return Ok(ApiResponse<object>.Ok(new
        {
            registration.RegistrationId,
            registration.PaymentMode,
            registration.PaidAmount,
            TotalFee = totalFee,
            RemainingAmount = Math.Max(0, totalFee - registration.PaidAmount),
            TuitionStatus = TuitionCalculator.GetTuitionStatus(registration),
            NextTuitionDueDate = TuitionCalculator.GetNextDueDate(registration),
            IsTuitionDue = TuitionCalculator.IsTuitionDue(registration),
            TuitionDueStatus = TuitionCalculator.GetDueStatus(registration),
            registration.LastPaymentAt,
            registration.TuitionNote
        }, "Cáº­p nháº­t há»c phÃ­ thÃ nh cÃ´ng"));
    }

    [HttpPost("{id}/convert-to-student")]
    public async Task<IActionResult> ConvertToStudent(int id)
    {
        var registration = await _db.CourseRegistrations
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.RegistrationId == id);

        if (registration == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy đăng ký"));

        if (registration.StudentId.HasValue)
        {
            registration.Status = "Đã nhập học";
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { StudentId = registration.StudentId.Value }, "Đăng ký đã có học sinh"));
        }

        var student = new Student
        {
            FullName = registration.FullName.Trim(),
            Email = string.IsNullOrWhiteSpace(registration.Email) ? null : registration.Email.Trim(),
            Phone = registration.Phone.Trim(),
            Note = registration.Note,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Students.Add(student);
        await _db.SaveChangesAsync();

        student.HashCode = HashCodeGenerator.Generate(nameof(Student), student.StudentId);
        registration.StudentId = student.StudentId;
        registration.Status = "Đã nhập học";
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { StudentId = student.StudentId }, "Đã chuyển đăng ký thành học sinh"));
    }

    [HttpGet("{id}/consultations")]
    public async Task<IActionResult> GetConsultations(int id)
    {
        if (!await _db.CourseRegistrations.AnyAsync(r => r.RegistrationId == id))
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy đăng ký"));

        var items = await _db.ConsultationLogs
            .Where(log => log.RegistrationId == id)
            .OrderByDescending(log => log.ContactedAt)
            .Select(log => new
            {
                log.ConsultationLogId,
                log.RegistrationId,
                log.ContactedAt,
                log.Channel,
                log.StaffName,
                log.Result,
                log.Note,
                log.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpPost("{id}/consultations")]
    public async Task<IActionResult> AddConsultation(int id, [FromBody] CreateConsultationLogRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        if (!await _db.CourseRegistrations.AnyAsync(r => r.RegistrationId == id))
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy đăng ký"));

        var log = new ConsultationLog
        {
            RegistrationId = id,
            ContactedAt = request.ContactedAt ?? DateTime.UtcNow,
            Channel = string.IsNullOrWhiteSpace(request.Channel) ? "Điện thoại" : request.Channel.Trim(),
            StaffName = string.IsNullOrWhiteSpace(request.StaffName) ? null : request.StaffName.Trim(),
            Result = request.Result.Trim(),
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
        };

        _db.ConsultationLogs.Add(log);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { log.ConsultationLogId }, "Đã thêm lịch sử tư vấn"));
    }

    /// <summary>
    /// DELETE /api/registrations/{id} - Xóa đăng ký
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var registration = await _db.CourseRegistrations.FindAsync(id);
        if (registration == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy đăng ký"));

        _db.CourseRegistrations.Remove(registration);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { registration.RegistrationId }, "Đã xóa đăng ký"));
    }
}
