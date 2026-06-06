using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeoEducation.Api.Models;

public class Course
{
    [Key]
    public int CourseId { get; set; }

    [Required]
    [StringLength(255)]
    public string CourseName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [ForeignKey("Subject")]
    public int? SubjectId { get; set; }

    [ForeignKey("Instructor")]
    public int? InstructorId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Price { get; set; }

    // Navigation
    public Subject? Subject { get; set; }
    public Instructor? Instructor { get; set; }
    public ICollection<CourseRegistration> Registrations { get; set; } = new List<CourseRegistration>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
