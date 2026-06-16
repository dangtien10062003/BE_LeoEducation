using System.ComponentModel.DataAnnotations.Schema;

namespace LeoEducation.Api.Models;

public class ClassStudent
{
    public int ClassId { get; set; }
    public int RegistrationId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TeachingClass Class { get; set; } = null!;
    public CourseRegistration Registration { get; set; } = null!;
}
