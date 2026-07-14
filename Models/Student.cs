using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeoEducation.Api.Models;

public class Student
{
    [Key]
    [Column("studentId")]
    public int StudentId { get; set; }

    [Column("hashCode")]
    [StringLength(128)]
    public string? HashCode { get; set; }

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

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string Status { get; set; } = "Active";

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CourseRegistration> CourseRegistrations { get; set; } = new List<CourseRegistration>();
}
