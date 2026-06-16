using System.ComponentModel.DataAnnotations;

namespace LeoEducation.Api.Models;

public class Blog
{
    [Key]
    public int Id { get; set; }

    [StringLength(128)]
    public string? HashCode { get; set; }

    [Required]
    [StringLength(250)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Summary { get; set; }

    public string? Content { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [StringLength(100)]
    public string? Author { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
