using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeoEducation.Api.Models;

public class Testimonial
{
    [Key]
    [Column("testimonialId")]
    public int TestimonialId { get; set; }

    [Column("hashCode")]
    [StringLength(128)]
    public string? HashCode { get; set; }

    [Required]
    [Column("studentName")]
    [StringLength(255)]
    public string StudentName { get; set; } = string.Empty;

    [Column("jobTitle")]
    [StringLength(255)]
    public string? JobTitle { get; set; }

    [Required]
    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("rating")]
    public int Rating { get; set; } = 5;

    [Column("avatarURL")]
    [StringLength(500)]
    public string? AvatarURL { get; set; }

    [Column("isActive")]
    public bool IsActive { get; set; } = true;

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
