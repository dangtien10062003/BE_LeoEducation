using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeoEducation.Api.Models;

public class CourseRegistration
{
    [Key]
    [Column("registrationId")]
    public int RegistrationId { get; set; }

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

    [Required]
    [ForeignKey("Course")]
    [Column("courseId")]
    public int CourseId { get; set; }

    [ForeignKey("Student")]
    [Column("studentId")]
    public int? StudentId { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string Status { get; set; } = "Mới";

    [Column("source")]
    [StringLength(50)]
    public string Source { get; set; } = "Website";

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [Column("paymentMode")]
    [StringLength(50)]
    public string? PaymentMode { get; set; }

    [Column("paidAmount", TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; }

    [Column("lastPaymentAt")]
    public DateTime? LastPaymentAt { get; set; }

    [Column("tuitionNote")]
    [StringLength(500)]
    public string? TuitionNote { get; set; }

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Course Course { get; set; } = null!;
    public Student? Student { get; set; }
    public ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();
    public ICollection<ConsultationLog> ConsultationLogs { get; set; } = new List<ConsultationLog>();
}
