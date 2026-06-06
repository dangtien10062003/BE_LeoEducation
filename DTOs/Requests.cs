using System.ComponentModel.DataAnnotations;

namespace LeoEducation.Api.DTOs;

// ===== Contact DTOs =====
public class CreateContactRequest
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập nội dung")]
    [MinLength(10, ErrorMessage = "Nội dung phải có ít nhất 10 ký tự")]
    public string Message { get; set; } = string.Empty;
}

public class UpdateContactStatusRequest
{
    [Required(ErrorMessage = "Vui lòng nhập trạng thái")]
    [RegularExpression("New|Processing|Resolved", ErrorMessage = "Trạng thái phải là: New, Processing, hoặc Resolved")]
    public string Status { get; set; } = string.Empty;
}

// ===== Subject DTOs =====
public class CreateSubjectRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tên môn học")]
    [StringLength(150)]
    public string SubjectName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateSubjectRequest
{
    [StringLength(150)]
    public string? SubjectName { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool? IsActive { get; set; }
}

// ===== Course DTOs =====
public class CreateCourseRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tên khóa học")]
    [StringLength(255)]
    public string CourseName { get; set; } = string.Empty;

    public string? Description { get; set; }
    public int? SubjectId { get; set; }
    public int? InstructorId { get; set; }

    [Range(0, 999999999.99)]
    public decimal? Price { get; set; }
}

public class UpdateCourseRequest
{
    [StringLength(255)]
    public string? CourseName { get; set; }
    public string? Description { get; set; }
    public int? SubjectId { get; set; }
    public int? InstructorId { get; set; }

    [Range(0, 999999999.99)]
    public decimal? Price { get; set; }
}

// ===== Registration DTOs =====
public class CreateRegistrationRequest
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(255)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(255)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn khóa học")]
    [Range(1, int.MaxValue, ErrorMessage = "Khóa học không hợp lệ")]
    public int CourseId { get; set; }
}

public class UpdateRegistrationStatusRequest
{
    [Required(ErrorMessage = "Vui lòng nhập trạng thái")]
    [RegularExpression("Mới|Đã gọi|Đã nhập học|Pending|Approved|Cancelled", ErrorMessage = "Trạng thái không hợp lệ")]
    public string Status { get; set; } = string.Empty;
}

// ===== Testimonial DTOs =====
public class CreateTestimonialRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tên học viên")]
    [StringLength(255)]
    public string StudentName { get; set; } = string.Empty;

    [StringLength(255)]
    public string? JobTitle { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung")]
    public string Content { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5")]
    public int Rating { get; set; } = 5;

    [StringLength(500)]
    public string? AvatarURL { get; set; }
}

// ===== Instructor DTOs =====
public class CreateInstructorRequest
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Role { get; set; }
    public string? Bio { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    [Range(0, 5)]
    public decimal Rating { get; set; } = 5.0m;

    [StringLength(50)]
    public string? Experience { get; set; }
}

// ===== Blog DTOs =====
public class CreateBlogRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
    [StringLength(250)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Summary { get; set; }
    public string? Content { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [StringLength(100)]
    public string? Author { get; set; }
}

// ===== Pagination Query =====
public class PaginationQuery
{
    private int _page = 1;
    private int _limit = 10;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int Limit
    {
        get => _limit;
        set => _limit = value < 1 ? 1 : (value > 100 ? 100 : value);
    }

    public int Offset => (Page - 1) * Limit;
}

public class CourseFilterQuery : PaginationQuery
{
    public string? Keyword { get; set; }
    public string? Category { get; set; }
    public int? SubjectId { get; set; }
}

// ===== Response Wrappers =====
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Thành công")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message)
        => new() { Success = false, Message = message };
}

public class PagedResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<T> Data { get; set; } = new();
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }

    public static PagedResponse<T> Ok(List<T> data, int page, int limit, int total, string message = "Thành công")
        => new()
        {
            Success = true,
            Message = message,
            Data = data,
            Page = page,
            Limit = limit,
            Total = total,
            TotalPages = (int)Math.Ceiling((double)total / limit)
        };
}
