using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeoEducation.Api.Models;

public class RecruitmentJob
{
    public int Id { get; set; }
    [JsonIgnore] public string TranslationsJson { get; set; } = "{}";
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public JsonElement Translations => JsonSerializer.Deserialize<JsonElement>(TranslationsJson);
    [MaxLength(20)] public string Department { get; set; } = "teaching";
    [MaxLength(20)] public string EmploymentType { get; set; } = "part-time";
    [MaxLength(20)] public string WorkMode { get; set; } = "remote";
    public DateTime? ClosesAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class JobApplication
{
    public int Id { get; set; }
    public int JobId { get; set; }
    [MaxLength(100)] public string FullName { get; set; } = "";
    [MaxLength(254)] public string Email { get; set; } = "";
    [MaxLength(30)] public string Phone { get; set; } = "";
    [MaxLength(2000)] public string ResumeUrl { get; set; } = "";
    [MaxLength(5000)] public string CoverLetter { get; set; } = "";
    [MaxLength(2)] public string Locale { get; set; } = "vi";
    [MaxLength(20)] public string Status { get; set; } = "new";
    public DateTime ConsentAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
