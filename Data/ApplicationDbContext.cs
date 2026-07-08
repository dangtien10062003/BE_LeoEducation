using LeoEducation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LeoEducation.Api.Data;

public class ApplicationDbContext : DbContext
{
    private const string CurrentTimestampSql = "CURRENT_TIMESTAMP";
    private static readonly DateTime SeedCreatedAt = new(2026, 1, 1);

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
        configurationBuilder.Properties<DateTime?>().HaveColumnType("timestamp without time zone");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<CourseRegistration> CourseRegistrations => Set<CourseRegistration>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<User> Users => Set<User>();
    public DbSet<TeachingClass> Classes => Set<TeachingClass>();
    public DbSet<ClassStudent> ClassStudents => Set<ClassStudent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== Subjects =====
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("Subjects");
            entity.HasKey(e => e.SubjectId);
            entity.Property(e => e.SubjectId).HasColumnName("subjectId");
            entity.Property(e => e.HashCode).HasColumnName("hashCode").HasMaxLength(128);
            entity.Property(e => e.SubjectName).HasColumnName("subjectName").IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasColumnName("imageUrl").HasMaxLength(500);
            entity.Property(e => e.IsActive).HasColumnName("isActive").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql(CurrentTimestampSql);
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt").HasDefaultValueSql(CurrentTimestampSql);
            entity.HasData(
                new Subject { SubjectId = 1, SubjectName = "Tiếng Anh", Description = "Các khóa học tiếng Anh", IsActive = true, CreatedAt = SeedCreatedAt, UpdatedAt = SeedCreatedAt },
                new Subject { SubjectId = 2, SubjectName = "Toán", Description = "Các khóa học toán học", IsActive = true, CreatedAt = SeedCreatedAt, UpdatedAt = SeedCreatedAt },
                new Subject { SubjectId = 3, SubjectName = "Vật lý", Description = "Các khóa học vật lý", IsActive = true, CreatedAt = SeedCreatedAt, UpdatedAt = SeedCreatedAt },
                new Subject { SubjectId = 4, SubjectName = "Hóa học", Description = "Các khóa học hóa học", IsActive = true, CreatedAt = SeedCreatedAt, UpdatedAt = SeedCreatedAt },
                new Subject { SubjectId = 5, SubjectName = "Ngữ văn", Description = "Các khóa học ngữ văn", IsActive = true, CreatedAt = SeedCreatedAt, UpdatedAt = SeedCreatedAt },
                new Subject { SubjectId = 6, SubjectName = "Sinh học", Description = "Các khóa học sinh học", IsActive = true, CreatedAt = SeedCreatedAt, UpdatedAt = SeedCreatedAt }
            );
        });

        // ===== Courses =====
        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Courses");
            entity.HasKey(e => e.CourseId);
            entity.Property(e => e.CourseId).HasColumnName("courseId");
            entity.Property(e => e.HashCode).HasColumnName("hashCode").HasMaxLength(128);
            entity.Property(e => e.CourseName).HasColumnName("courseName").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.SubjectId).HasColumnName("subjectId");
            entity.Property(e => e.InstructorId).HasColumnName("instructorId");
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql(CurrentTimestampSql);
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt").HasDefaultValueSql(CurrentTimestampSql);
            entity.HasOne(e => e.Subject)
                  .WithMany(s => s.Courses)
                  .HasForeignKey(e => e.SubjectId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Instructor)
                  .WithMany()
                  .HasForeignKey(e => e.InstructorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ===== Classes =====
        modelBuilder.Entity<TeachingClass>(entity =>
        {
            entity.ToTable("TeachingClasses");
            entity.HasKey(e => e.ClassId);
            entity.Property(e => e.ClassId).HasColumnName("classId");
            entity.Property(e => e.HashCode).HasColumnName("hashCode").HasMaxLength(128);
            entity.Property(e => e.ClassName).HasColumnName("className").IsRequired().HasMaxLength(255);
            entity.Property(e => e.CourseId).HasColumnName("courseId");
            entity.Property(e => e.SubjectId).HasColumnName("subjectId");
            entity.Property(e => e.InstructorId).HasColumnName("instructorId");
            entity.Property(e => e.StartDate).HasColumnName("startDate");
            entity.Property(e => e.EndDate).HasColumnName("endDate");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue("Active");
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql(CurrentTimestampSql);
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt").HasDefaultValueSql(CurrentTimestampSql);

            entity.HasOne(e => e.Course)
                  .WithMany(c => c.Classes)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Subject)
                  .WithMany()
                  .HasForeignKey(e => e.SubjectId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Instructor)
                  .WithMany()
                  .HasForeignKey(e => e.InstructorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ClassStudent>(entity =>
        {
            entity.ToTable("TeachingClassStudents");
            entity.HasKey(e => new { e.ClassId, e.RegistrationId });
            entity.Property(e => e.ClassId).HasColumnName("classId");
            entity.Property(e => e.RegistrationId).HasColumnName("registrationId");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql(CurrentTimestampSql);

            entity.HasOne(e => e.Class)
                  .WithMany(c => c.Students)
                  .HasForeignKey(e => e.ClassId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Registration)
                  .WithMany(r => r.ClassStudents)
                  .HasForeignKey(e => e.RegistrationId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // ===== CourseRegistrations =====
        modelBuilder.Entity<CourseRegistration>(entity =>
        {
            entity.ToTable("CourseRegistrations");
            entity.HasKey(e => e.RegistrationId);
            entity.Property(e => e.RegistrationId).HasColumnName("registrationId");
            entity.Property(e => e.HashCode).HasColumnName("hashCode").HasMaxLength(128);
            entity.Property(e => e.FullName).HasColumnName("fullName").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.Phone).HasColumnName("phone").IsRequired().HasMaxLength(20);
            entity.Property(e => e.CourseId).HasColumnName("courseId");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue("Mới");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql(CurrentTimestampSql);
            entity.HasOne(e => e.Course)
                  .WithMany(c => c.Registrations)
                  .HasForeignKey(e => e.CourseId)
                  .HasConstraintName("FK_Registration_Course");
        });

        // ===== Testimonials =====
        modelBuilder.Entity<Testimonial>(entity =>
        {
            entity.ToTable("Testimonials");
            entity.HasKey(e => e.TestimonialId);
            entity.Property(e => e.TestimonialId).HasColumnName("testimonialId");
            entity.Property(e => e.HashCode).HasColumnName("hashCode").HasMaxLength(128);
            entity.Property(e => e.StudentName).HasColumnName("studentName").IsRequired().HasMaxLength(255);
            entity.Property(e => e.JobTitle).HasColumnName("jobTitle").HasMaxLength(255);
            entity.Property(e => e.Content).HasColumnName("content").IsRequired();
            entity.Property(e => e.Rating).HasColumnName("rating").HasDefaultValue(5);
            entity.Property(e => e.AvatarURL).HasColumnName("avatarURL").HasMaxLength(500);
            entity.Property(e => e.IsActive).HasColumnName("isActive").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql(CurrentTimestampSql);
        });

        // ===== Instructors =====
        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.ToTable("Instructors");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HashCode).HasColumnName("hashCode").HasMaxLength(128);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(100);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.Rating).HasColumnType("decimal(3,2)").HasDefaultValue(5.0m);
            entity.Property(e => e.Experience).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // ===== ContactRequests =====
        modelBuilder.Entity<ContactRequest>(entity =>
        {
            entity.ToTable("ContactRequests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HashCode).HasColumnName("hashCode").HasMaxLength(128);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("New");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(CurrentTimestampSql);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql(CurrentTimestampSql);
        });

        // ===== Blogs =====
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.ToTable("Blogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HashCode).HasColumnName("hashCode").HasMaxLength(128);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Summary).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Author).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(CurrentTimestampSql);
        });

        // ===== Users (existing table) =====
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("userId");
            entity.Property(e => e.FullName).HasColumnName("fullName").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.Phone).HasColumnName("phone").IsRequired().HasMaxLength(20);
            entity.Property(e => e.PasswordHash).HasColumnName("passwordHash").IsRequired().HasMaxLength(500);
            entity.Property(e => e.AvatarURL).HasColumnName("avatarURL").HasMaxLength(500);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
        });
    }
}
