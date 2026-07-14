using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeoEducation.Api.Data;
using LeoEducation.Api.DTOs;
using LeoEducation.Api.Models;
using LeoEducation.Api.Utils;

namespace LeoEducation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ClassesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ClassFilterQuery request)
    {
        var query = _db.Classes
            .Include(c => c.Course)
            .Include(c => c.Subject)
            .Include(c => c.Instructor)
            .Include(c => c.Students)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(c => c.ClassName.ToLower().Contains(keyword)
                                  || c.Course.CourseName.ToLower().Contains(keyword)
                                  || (c.Subject != null && c.Subject.SubjectName.ToLower().Contains(keyword))
                                  || (c.Instructor != null && c.Instructor.FullName.ToLower().Contains(keyword)));
        }

        if (request.CourseId.HasValue)
            query = query.Where(c => c.CourseId == request.CourseId.Value);
        if (request.SubjectId.HasValue)
            query = query.Where(c => c.SubjectId == request.SubjectId.Value);
        if (request.InstructorId.HasValue)
            query = query.Where(c => c.InstructorId == request.InstructorId.Value);

        var today = DateTime.UtcNow.Date;
        if (!string.IsNullOrWhiteSpace(request.TeachingStatus))
        {
            var status = request.TeachingStatus.Trim().ToLower();
            query = status switch
            {
                "teaching" or "dangday" or "đang dạy" => query.Where(c => c.StartDate.Date <= today && c.EndDate.Date >= today),
                "upcoming" or "sapmo" or "sắp mở" => query.Where(c => c.StartDate.Date > today),
                "finished" or "ketthuc" or "đã kết thúc" => query.Where(c => c.EndDate.Date < today),
                _ => query
            };
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.StartDate)
            .Skip(request.Offset)
            .Take(request.PageSize)
            .Select(c => new
            {
                c.ClassId,
                c.HashCode,
                c.ClassName,
                c.CourseId,
                CourseName = c.Course.CourseName,
                c.SubjectId,
                SubjectName = c.Subject != null ? c.Subject.SubjectName : c.Course.Subject != null ? c.Course.Subject.SubjectName : null,
                c.InstructorId,
                InstructorName = c.Instructor != null ? c.Instructor.FullName : c.Course.Instructor != null ? c.Course.Instructor.FullName : null,
                c.StartDate,
                c.EndDate,
                c.Status,
                TeachingStatus = GetTeachingStatus(c.StartDate, c.EndDate, today),
                StudentCount = c.Students.Count,
                c.Note,
                c.CreatedAt,
                c.UpdatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            message = "Thành công",
            data = items,
            pageIndex = request.PageIndex,
            pageSize = request.PageSize,
            page = request.PageIndex,
            limit = request.PageSize,
            total,
            totalPages = (int)Math.Ceiling((double)total / request.PageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var today = DateTime.UtcNow.Date;
        var item = await _db.Classes
            .Include(c => c.Course).ThenInclude(c => c.Subject)
            .Include(c => c.Course).ThenInclude(c => c.Instructor)
            .Include(c => c.Subject)
            .Include(c => c.Instructor)
            .Include(c => c.Students).ThenInclude(s => s.Registration)
            .Where(c => c.ClassId == id)
            .Select(c => new
            {
                c.ClassId,
                c.HashCode,
                c.ClassName,
                c.CourseId,
                CourseName = c.Course.CourseName,
                c.SubjectId,
                SubjectName = c.Subject != null ? c.Subject.SubjectName : c.Course.Subject != null ? c.Course.Subject.SubjectName : null,
                c.InstructorId,
                InstructorName = c.Instructor != null ? c.Instructor.FullName : c.Course.Instructor != null ? c.Course.Instructor.FullName : null,
                c.StartDate,
                c.EndDate,
                c.Status,
                TeachingStatus = GetTeachingStatus(c.StartDate, c.EndDate, today),
                c.Note,
                Students = c.Students
                    .OrderBy(s => s.Registration.FullName)
                    .Select(s => new
                    {
                        s.RegistrationId,
                        s.Registration.FullName,
                        s.Registration.Email,
                        s.Registration.Phone,
                        s.Registration.Status,
                        s.Registration.Note,
                        s.Registration.CreatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (item == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy lớp học"));

        return Ok(ApiResponse<object>.Ok(item));
    }

    [HttpGet("available-students")]
    public async Task<IActionResult> GetAvailableStudents([FromQuery] int courseId, [FromQuery] int? classId)
    {
        if (courseId < 1)
            return BadRequest(ApiResponse<object>.Fail("Vui lòng chọn khóa học"));

        var registrations = await _db.CourseRegistrations
            .Where(r => r.CourseId == courseId)
            .Select(r => new
            {
                r.RegistrationId,
                r.FullName,
                r.Email,
                r.Phone,
                r.Note,
                r.Status,
                r.CreatedAt,
                Classes = r.ClassStudents
                    .Where(cs => cs.Class.CourseId == courseId)
                    .Select(cs => new { cs.ClassId, cs.Class.ClassName })
                    .ToList()
            })
            .OrderBy(r => r.FullName)
            .ToListAsync();

        var items = registrations.Select(r =>
        {
            var enrolled = IsEnrolled(r.Status);
            var inCurrentClass = classId.HasValue && r.Classes.Any(c => c.ClassId == classId.Value);
            var otherClass = r.Classes.FirstOrDefault(c => !classId.HasValue || c.ClassId != classId.Value);

            return new
            {
                r.RegistrationId,
                r.FullName,
                r.Email,
                r.Phone,
                r.Note,
                RegistrationStatus = r.Status,
                EnrollmentStatus = enrolled ? "Đã ghi danh" : "Chờ ghi danh",
                StudentStatus = (inCurrentClass || otherClass != null) ? "Đã có lớp" : "Chưa có lớp",
                ClassAssignmentStatus = inCurrentClass
                    ? "Đã trong lớp này"
                    : otherClass != null
                        ? "Đã có lớp"
                        : enrolled ? "Chờ xếp lớp" : "Chờ ghi danh",
                AssignedClassId = otherClass?.ClassId,
                AssignedClassName = otherClass?.ClassName,
                CanAdd = enrolled && (otherClass == null || inCurrentClass),
                r.CreatedAt
            };
        }).ToList();

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request)
    {
        var validation = await ValidateRequest(request);
        if (validation != null) return validation;

        var course = await _db.Courses.FirstAsync(c => c.CourseId == request.CourseId);
        var item = new TeachingClass
        {
            ClassName = request.ClassName.Trim(),
            CourseId = request.CourseId,
            SubjectId = request.SubjectId ?? course.SubjectId,
            InstructorId = request.InstructorId ?? course.InstructorId,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
            Note = request.Note,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var registrationId in request.RegistrationIds.Distinct())
            item.Students.Add(new ClassStudent { RegistrationId = registrationId });

        _db.Classes.Add(item);
        await _db.SaveChangesAsync();

        item.HashCode = HashCodeGenerator.Generate(nameof(TeachingClass), item.ClassId);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { item.ClassId }, "Tạo lớp học thành công"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassRequest request)
    {
        var item = await _db.Classes
            .Include(c => c.Students)
            .FirstOrDefaultAsync(c => c.ClassId == id);

        if (item == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy lớp học"));

        var validation = await ValidateRequest(request, id);
        if (validation != null) return validation;

        var course = await _db.Courses.FirstAsync(c => c.CourseId == request.CourseId);
        item.ClassName = request.ClassName.Trim();
        item.CourseId = request.CourseId;
        item.SubjectId = request.SubjectId ?? course.SubjectId;
        item.InstructorId = request.InstructorId ?? course.InstructorId;
        item.StartDate = request.StartDate.Date;
        item.EndDate = request.EndDate.Date;
        item.Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim();
        item.Note = request.Note;
        item.UpdatedAt = DateTime.UtcNow;

        _db.ClassStudents.RemoveRange(item.Students);
        foreach (var registrationId in request.RegistrationIds.Distinct())
            item.Students.Add(new ClassStudent { ClassId = id, RegistrationId = registrationId });

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { item.ClassId }, "Cập nhật lớp học thành công"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Classes.FindAsync(id);
        if (item == null)
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy lớp học"));

        _db.Classes.Remove(item);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { item.ClassId }, "Đã xóa lớp học"));
    }

    private async Task<IActionResult?> ValidateRequest(CreateClassRequest request, int? currentClassId = null)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(ApiResponse<object>.Fail(string.Join("; ", errors)));
        }

        if (request.EndDate.Date < request.StartDate.Date)
            return BadRequest(ApiResponse<object>.Fail("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu"));

        if (!await _db.Courses.AnyAsync(c => c.CourseId == request.CourseId))
            return BadRequest(ApiResponse<object>.Fail("Khóa học không tồn tại"));

        if (request.SubjectId.HasValue && !await _db.Subjects.AnyAsync(s => s.SubjectId == request.SubjectId.Value))
            return BadRequest(ApiResponse<object>.Fail("Môn học không tồn tại"));

        if (request.InstructorId.HasValue && !await _db.Instructors.AnyAsync(i => i.Id == request.InstructorId.Value))
            return BadRequest(ApiResponse<object>.Fail("Giáo viên không tồn tại"));

        var registrationIds = request.RegistrationIds.Distinct().ToList();
        if (registrationIds.Count == 0) return null;

        var registrations = await _db.CourseRegistrations
            .Where(r => registrationIds.Contains(r.RegistrationId))
            .Select(r => new { r.RegistrationId, r.CourseId, r.Status, r.FullName })
            .ToListAsync();

        if (registrations.Count != registrationIds.Count || registrations.Any(r => r.CourseId != request.CourseId))
            return BadRequest(ApiResponse<object>.Fail("Danh sách học sinh có bản ghi không thuộc khóa học đã chọn"));

        var notEnrolled = registrations.Where(r => !IsEnrolled(r.Status)).Select(r => r.FullName).ToList();
        if (notEnrolled.Any())
            return BadRequest(ApiResponse<object>.Fail($"Chỉ được thêm học sinh đã ghi danh vào lớp: {string.Join(", ", notEnrolled)}"));

        var assignedElsewhere = await _db.ClassStudents
            .Include(cs => cs.Class)
            .Include(cs => cs.Registration)
            .Where(cs => registrationIds.Contains(cs.RegistrationId)
                      && cs.Class.CourseId == request.CourseId
                      && (!currentClassId.HasValue || cs.ClassId != currentClassId.Value))
            .Select(cs => new { cs.Registration.FullName, cs.Class.ClassName })
            .ToListAsync();

        if (assignedElsewhere.Any())
        {
            var names = assignedElsewhere.Select(x => $"{x.FullName} ({x.ClassName})");
            return BadRequest(ApiResponse<object>.Fail($"Học sinh đã thuộc lớp khác: {string.Join(", ", names)}"));
        }

        return null;
    }

    private static bool IsEnrolled(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return false;

        var value = status.Trim();
        return value.Equals("Đã nhập học", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetTeachingStatus(DateTime startDate, DateTime endDate, DateTime today)
    {
        if (startDate.Date > today) return "Sắp mở";
        if (endDate.Date < today) return "Đã kết thúc";
        return "Đang dạy";
    }
}
