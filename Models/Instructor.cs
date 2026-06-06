using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeoEducation.Api.Models;

public class Instructor
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Role { get; set; }

    public string? Bio { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal Rating { get; set; } = 5.0m;

    [StringLength(50)]
    public string? Experience { get; set; }

    public bool IsActive { get; set; } = true;
}
