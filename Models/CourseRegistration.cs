using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeoEducation.Api.Models;

public class CourseRegistration
{
    [Key]
    [Column("registrationId")]
    public int RegistrationId { get; set; }

    [Required]
    [Column("fullName")]
    [StringLength(255)]
    public string FullName { get; set; } = string.Empty;

    [Column("email")]
    [StringLength(255)]
    public string? Email { get; set; }

    [Required]
    [Column("phone")]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [ForeignKey("Course")]
    [Column("courseId")]
    public int CourseId { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string Status { get; set; } = "Mới";

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Course Course { get; set; } = null!;
}
