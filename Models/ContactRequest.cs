using System.ComponentModel.DataAnnotations;

namespace LeoEducation.Api.Models;

public class ContactRequest
{
    [Key]
    public int Id { get; set; }

    [StringLength(128)]
    public string? HashCode { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    public string? Message { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "New";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
