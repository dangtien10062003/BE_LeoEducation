using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeoEducation.Api.Models;

public class ConsultationLog
{
    [Key]
    [Column("consultationLogId")]
    public int ConsultationLogId { get; set; }

    [Column("registrationId")]
    public int RegistrationId { get; set; }

    [Column("contactedAt")]
    public DateTime ContactedAt { get; set; } = DateTime.UtcNow;

    [Column("channel")]
    [StringLength(50)]
    public string Channel { get; set; } = "Điện thoại";

    [Column("staffName")]
    [StringLength(100)]
    public string? StaffName { get; set; }

    [Column("result")]
    [StringLength(100)]
    public string Result { get; set; } = string.Empty;

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CourseRegistration Registration { get; set; } = null!;
}
