using System.ComponentModel.DataAnnotations;

namespace LeoEducation.Api.Models;

public class Subject
{
    [Key]
    public int SubjectId { get; set; }

    [Required]
    [StringLength(150)]
    public string SubjectName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
