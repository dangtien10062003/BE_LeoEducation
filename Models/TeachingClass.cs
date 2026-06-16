using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeoEducation.Api.Models;

public class TeachingClass
{
    [Key]
    public int ClassId { get; set; }

    [StringLength(128)]
    public string? HashCode { get; set; }

    [Required]
    [StringLength(255)]
    public string ClassName { get; set; } = string.Empty;

    [ForeignKey("Course")]
    public int CourseId { get; set; }

    [ForeignKey("Subject")]
    public int? SubjectId { get; set; }

    [ForeignKey("Instructor")]
    public int? InstructorId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Active";

    [StringLength(500)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Course Course { get; set; } = null!;
    public Subject? Subject { get; set; }
    public Instructor? Instructor { get; set; }
    public ICollection<ClassStudent> Students { get; set; } = new List<ClassStudent>();
}
