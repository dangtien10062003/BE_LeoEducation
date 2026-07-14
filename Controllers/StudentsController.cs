using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;
using LeoEducation.Api.Utils;

namespace LeoEducation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public StudentsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] StudentFilterQuery request)
    {
        var query = _db.Students
            .Include(s => s.CourseRegistrations)
                .ThenInclude(r => r.Course)
            .Include(s => s.CourseRegistrations)
                .ThenInclude(r => r.ClassStudents)
                .ThenInclude(cs => cs.Class)
            .AsQueryable();

        if (request.ClassId.HasValue)
            query = query.Where(s => s.CourseRegistrations.Any(r => r.ClassStudents.Any(cs => cs.ClassId == request.ClassId.Value)));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            var matchesAssignedStatus = "đã có lớp".Contains(keyword);
            var matchesUnassignedStatus = "chưa có lớp".Contains(keyword);

            query = query.Where(s => s.FullName.ToLower().Contains(keyword)
                                  || (s.Email != null && s.Email.ToLower().Contains(keyword))
                                  || s.Phone.ToLower().Contains(keyword)
                                  || (s.Note != null && s.Note.ToLower().Contains(keyword))
                                  || s.CourseRegistrations.Any(r => r.Course.CourseName.ToLower().Contains(keyword))
                                  || (matchesAssignedStatus && s.CourseRegistrations.Any(r => r.ClassStudents.Any()))
                                  || (matchesUnassignedStatus && !s.CourseRegistrations.Any(r => r.ClassStudents.Any())));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .Select(s => new
            {
                s.StudentId,
                s.FullName,
                s.Email,
                s.Phone,
                s.Note,
                s.Status,
                CourseId = s.CourseRegistrations
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => r.CourseId)
                    .FirstOrDefault(),
                CourseNames = s.CourseRegistrations
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => r.Course.CourseName)
                    .Distinct()
                    .ToList(),
                StudentStatus = s.CourseRegistrations.Any(r => r.ClassStudents.Any()) ? "Đã có lớp" : "Chưa có lớp",
                TuitionItems = s.CourseRegistrations
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new
                    {
                        r.RegistrationId,
                        r.CourseId,
                        CourseName = r.Course.CourseName,
                        BillingType = r.Course.BillingType,
                        PaymentMode = string.IsNullOrWhiteSpace(r.PaymentMode) ? r.Course.BillingType : r.PaymentMode,
                        DurationMonths = TuitionCalculator.GetDurationMonths(r.Course),
                        TotalFee = TuitionCalculator.GetTotalFee(r.Course),
                        r.PaidAmount,
                        RemainingAmount = Math.Max(0, TuitionCalculator.GetTotalFee(r.Course) - r.PaidAmount),
                        TuitionStatus = TuitionCalculator.GetTuitionStatus(r),
                        NextTuitionDueDate = TuitionCalculator.GetNextDueDate(r),
                        IsTuitionDue = TuitionCalculator.IsTuitionDue(r),
                        TuitionDueStatus = TuitionCalculator.GetDueStatus(r),
                        r.LastPaymentAt,
                        r.TuitionNote
                    })
                    .ToList(),
                TuitionStatus = s.CourseRegistrations
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => TuitionCalculator.GetTuitionStatus(r))
                    .FirstOrDefault() ?? "Chưa đóng",
                TotalFee = s.CourseRegistrations
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => TuitionCalculator.GetTotalFee(r.Course))
                    .FirstOrDefault(),
                PaidAmount = s.CourseRegistrations
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => r.PaidAmount)
                    .FirstOrDefault(),
                NextTuitionDueDate = s.CourseRegistrations
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => TuitionCalculator.GetNextDueDate(r))
                    .FirstOrDefault(),
                IsTuitionDue = s.CourseRegistrations
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => TuitionCalculator.IsTuitionDue(r))
                    .FirstOrDefault(),
                TuitionDueStatus = s.CourseRegistrations
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => TuitionCalculator.GetDueStatus(r))
                    .FirstOrDefault() ?? "Chưa đến hạn",
                AssignedClasses = s.CourseRegistrations
                    .SelectMany(r => r.ClassStudents)
                    .OrderBy(cs => cs.Class.ClassName)
                    .Select(cs => new
                    {
                        cs.ClassId,
                        cs.Class.ClassName
                    })
                    .Distinct()
                    .ToList(),
                s.CreatedAt,
                s.UpdatedAt
            })
            .ToListAsync();

        return Ok(PagedResponse<object>.Ok(items.Cast<object>().ToList(), request.PageIndex, request.PageSize, total));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpdateStudentRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var courseExists = await _db.Courses.AnyAsync(c => c.CourseId == request.CourseId);
        if (!courseExists)
            return BadRequest(ApiResponse<object>.Fail("Khóa học không tồn tại"));

        var now = DateTime.UtcNow;
        var student = new Student
        {
            FullName = request.FullName.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Phone = request.Phone.Trim(),
            Note = request.Note,
            Status = "Active",
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Students.Add(student);
        await _db.SaveChangesAsync();

        student.HashCode = HashCodeGenerator.Generate(nameof(Student), student.StudentId);
        _db.CourseRegistrations.Add(new CourseRegistration
        {
            FullName = student.FullName,
            Email = student.Email,
            Phone = student.Phone,
            CourseId = request.CourseId,
            StudentId = student.StudentId,
            Status = "Đã nhập học",
            Source = "Trực tiếp",
            Note = request.Note,
            CreatedAt = now
        });
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { student.StudentId }, "Thêm học sinh thành công"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        var student = await _db.Students
            .Include(s => s.CourseRegistrations)
            .FirstOrDefaultAsync(s => s.StudentId == id);

        if (student == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy học sinh"));

        var courseExists = await _db.Courses.AnyAsync(c => c.CourseId == request.CourseId);
        if (!courseExists)
            return BadRequest(ApiResponse<object>.Fail("Khóa học không tồn tại"));

        student.FullName = request.FullName.Trim();
        student.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        student.Phone = request.Phone.Trim();
        student.Note = request.Note;
        student.UpdatedAt = DateTime.UtcNow;

        foreach (var registration in student.CourseRegistrations)
        {
            registration.FullName = student.FullName;
            registration.Email = student.Email;
            registration.Phone = student.Phone;
            registration.Note = request.Note;
        }

        var latestRegistration = student.CourseRegistrations.OrderByDescending(r => r.CreatedAt).FirstOrDefault();
        if (latestRegistration == null || latestRegistration.CourseId != request.CourseId)
        {
            _db.CourseRegistrations.Add(new CourseRegistration
            {
                FullName = student.FullName,
                Email = student.Email,
                Phone = student.Phone,
                CourseId = request.CourseId,
                StudentId = student.StudentId,
                Status = "Đã nhập học",
                Source = "Trực tiếp",
                Note = request.Note,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { student.StudentId }, "Cập nhật học sinh thành công"));
    }
}
