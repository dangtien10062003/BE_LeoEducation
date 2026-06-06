using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeoEducation.Api.Models;

[Table("Users")]
public class User
{
    [Key]
    [Column("userId")]
    public int Id { get; set; }

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
    [Column("passwordHash")]
    [StringLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("avatarURL")]
    [StringLength(500)]
    public string? AvatarURL { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string Status { get; set; } = "Active";

    [Column("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [Column("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [Column("LastLoginAt")]
    public DateTime? LastLoginAt { get; set; }
}
