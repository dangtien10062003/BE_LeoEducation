using LeoEducation.Api.Data;
using LeoEducation.Api.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

LoadDotEnv(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
ConfigureRenderPort();
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "instructors"));
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "courses"));

var builder = WebApplication.CreateBuilder(args);

// ===== CORS =====
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin => IsAllowedCorsOrigin(origin, allowedOrigins))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ===== Database =====
var useInMemoryDatabase = builder.Configuration.GetValue("Database:UseInMemory", false);
if (useInMemoryDatabase)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("LeoEducationLocalDev"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? string.Empty;
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection configuration.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// ===== JWT Authentication =====
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? string.Empty;
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Missing Jwt:Key configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LeoEducation";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// ===== Controllers + JSON =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ===== Swagger =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ===== Global Error Handling Middleware =====
app.UseMiddleware<GlobalExceptionMiddleware>();

// ===== Swagger =====
var swaggerEnabled = builder.Configuration.GetValue("Swagger:Enabled", app.Environment.IsDevelopment());
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LeoEducation API v1");
        c.RoutePrefix = string.Empty;
    });
}

// ===== Auto-migrate on startup =====
var autoMigrate = builder.Configuration.GetValue("Database:AutoMigrate", false);
if (useInMemoryDatabase)
{
    await SeedLocalDevelopmentDataAsync(app.Services);
}
else if (autoMigrate)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowReactFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();

static void LoadDotEnv(string path)
{
    if (!File.Exists(path))
        return;

    foreach (var rawLine in File.ReadAllLines(path))
    {
        var line = rawLine.Trim();
        if (line.Length == 0 || line.StartsWith('#'))
            continue;

        var separatorIndex = line.IndexOf('=');
        if (separatorIndex <= 0)
            continue;

        var key = line[..separatorIndex].Trim();
        var value = line[(separatorIndex + 1)..].Trim().Trim('"');

        if (!string.IsNullOrWhiteSpace(key) && Environment.GetEnvironmentVariable(key) is null)
            Environment.SetEnvironmentVariable(key, value);
    }
}

static void ConfigureRenderPort()
{
    if (Environment.GetEnvironmentVariable("ASPNETCORE_URLS") is not null)
        return;

    var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
    Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://0.0.0.0:{port}");
}

static bool IsAllowedCorsOrigin(string origin, string[] configuredOrigins)
{
    if (configuredOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
        return true;

    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        return false;

    return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
        || uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase);
}

static async Task SeedLocalDevelopmentDataAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (await db.Users.AnyAsync())
        return;

    var now = DateTime.UtcNow;
    var today = DateTime.UtcNow.Date;
    var instructors = new[]
    {
        new LeoEducation.Api.Models.Instructor { Id = 1, FullName = "Nguyễn Minh Anh", Role = "Giảng viên IELTS", Bio = "Dữ liệu local dev", Rating = 5, Experience = "6 năm", IsActive = true },
        new LeoEducation.Api.Models.Instructor { Id = 2, FullName = "Trần Quốc Bảo", Role = "Giảng viên Toán", Bio = "Dữ liệu local dev", Rating = 4.8m, Experience = "8 năm", IsActive = true },
    };
    var subjects = new[]
    {
        new LeoEducation.Api.Models.Subject { SubjectId = 1, SubjectName = "Tiếng Anh", Description = "Các khóa tiếng Anh", IsActive = true, CreatedAt = now, UpdatedAt = now },
        new LeoEducation.Api.Models.Subject { SubjectId = 2, SubjectName = "Toán", Description = "Các khóa toán học", IsActive = true, CreatedAt = now, UpdatedAt = now },
    };
    var courses = new[]
    {
        new LeoEducation.Api.Models.Course { CourseId = 1, CourseName = "Tiếng Anh giao tiếp", Description = "Khóa học mẫu cho local dev", SubjectId = 1, InstructorId = 1, Price = 1500000, BillingType = "Monthly", StartDate = new DateTime(2026, 7, 1), EndDate = new DateTime(2026, 9, 30), CreatedAt = now, UpdatedAt = now },
        new LeoEducation.Api.Models.Course { CourseId = 2, CourseName = "IELTS Foundation", Description = "Khóa nền tảng IELTS", SubjectId = 1, InstructorId = 1, Price = 3200000, BillingType = "FullCourse", StartDate = new DateTime(2026, 7, 15), EndDate = new DateTime(2026, 10, 15), CreatedAt = now, UpdatedAt = now },
        new LeoEducation.Api.Models.Course { CourseId = 3, CourseName = "Toán tư duy", Description = "Khóa toán tư duy", SubjectId = 2, InstructorId = 2, Price = 2200000, BillingType = "Monthly", StartDate = new DateTime(2026, 7, 1), EndDate = new DateTime(2026, 8, 31), CreatedAt = now, UpdatedAt = now },
    };
    var classes = new[]
    {
        new LeoEducation.Api.Models.TeachingClass { ClassId = 1, ClassName = "TA-GT-01", CourseId = 1, SubjectId = 1, InstructorId = 1, StartDate = today.AddDays(-7), EndDate = today.AddMonths(2), Status = "Active", Note = "Lớp mẫu local dev", CreatedAt = now, UpdatedAt = now },
        new LeoEducation.Api.Models.TeachingClass { ClassId = 2, ClassName = "IELTS-FD-01", CourseId = 2, SubjectId = 1, InstructorId = 1, StartDate = today.AddDays(5), EndDate = today.AddMonths(3), Status = "Active", Note = "Lớp sắp mở", CreatedAt = now, UpdatedAt = now },
        new LeoEducation.Api.Models.TeachingClass { ClassId = 3, ClassName = "TOAN-TD-01", CourseId = 3, SubjectId = 2, InstructorId = 2, StartDate = today.AddDays(-20), EndDate = today.AddMonths(1), Status = "Active", Note = "Lớp toán mẫu", CreatedAt = now, UpdatedAt = now },
    };
    var students = new[]
    {
        new LeoEducation.Api.Models.Student { StudentId = 1, FullName = "Nguyễn Văn A", Email = "student1@example.com", Phone = "0900000001", Note = "Đã xếp lớp mẫu", Status = "Active", CreatedAt = today.AddDays(-2).AddHours(9), UpdatedAt = now },
        new LeoEducation.Api.Models.Student { StudentId = 2, FullName = "Trần Thị B", Email = "student2@example.com", Phone = "0900000002", Note = "Chưa xếp lớp để kiểm tra trạng thái", Status = "Active", CreatedAt = today.AddDays(-1).AddHours(10), UpdatedAt = now },
        new LeoEducation.Api.Models.Student { StudentId = 3, FullName = "Võ Ngọc E", Email = "student5@example.com", Phone = "0900000005", Note = "Đã xếp lớp toán", Status = "Active", CreatedAt = today.AddDays(-6).AddHours(16), UpdatedAt = now },
        new LeoEducation.Api.Models.Student { StudentId = 4, FullName = "Hoàng Kim H", Email = "student8@example.com", Phone = "0900000008", Note = "Đã xếp lớp IELTS", Status = "Active", CreatedAt = today.AddMonths(-1).AddDays(3).AddHours(9), UpdatedAt = now },
        new LeoEducation.Api.Models.Student { StudentId = 5, FullName = "Ngô Bảo L", Email = "student11@example.com", Phone = "0900000011", Note = "Học thử đạt yêu cầu", Status = "Active", CreatedAt = today.AddMonths(-5).AddDays(8).AddHours(13), UpdatedAt = now },
    };
    var registrations = new[]
    {
        new LeoEducation.Api.Models.CourseRegistration { RegistrationId = 1, StudentId = 1, FullName = "Nguyễn Văn A", Email = "student1@example.com", Phone = "0900000001", CourseId = 1, Status = "Đã nhập học", Source = "Website", Note = "Đã xếp lớp mẫu", PaymentMode = "Monthly", PaidAmount = 1500000, LastPaymentAt = today.AddDays(-2), TuitionNote = "Đã đóng tháng 7", CreatedAt = today.AddDays(-2).AddHours(9) },
        new LeoEducation.Api.Models.CourseRegistration { RegistrationId = 2, StudentId = 2, FullName = "Trần Thị B", Email = "student2@example.com", Phone = "0900000002", CourseId = 1, Status = "Đã nhập học", Source = "Facebook", Note = "Chưa xếp lớp để kiểm tra trạng thái", PaymentMode = "Monthly", PaidAmount = 0, CreatedAt = today.AddDays(-1).AddHours(10) },
        new LeoEducation.Api.Models.CourseRegistration { RegistrationId = 3, FullName = "Lê Hoàng C", Email = "student3@example.com", Phone = "0900000003", CourseId = 2, Status = "Mới", Source = "Zalo", Note = "Phụ huynh cần tư vấn lịch tối", CreatedAt = today.AddHours(8) },
        new LeoEducation.Api.Models.CourseRegistration { RegistrationId = 4, FullName = "Phạm Minh D", Email = "student4@example.com", Phone = "0900000004", CourseId = 2, Status = "Đã gọi", Source = "Facebook", Note = "Hẹn gọi lại cuối tuần", CreatedAt = today.AddDays(-3).AddHours(14) },
        new LeoEducation.Api.Models.CourseRegistration { RegistrationId = 5, StudentId = 3, FullName = "Võ Ngọc E", Email = "student5@example.com", Phone = "0900000005", CourseId = 3, Status = "Đã nhập học", Source = "Giới thiệu", Note = "Đã xếp lớp toán", PaymentMode = "Monthly", PaidAmount = 4400000, LastPaymentAt = today.AddDays(-6), TuitionNote = "Đã đóng đủ 2 tháng", CreatedAt = today.AddDays(-6).AddHours(16) },
        new LeoEducation.Api.Models.CourseRegistration { RegistrationId = 6, FullName = "Đặng Gia F", Email = "student6@example.com", Phone = "0900000006", CourseId = 3, Status = "Mới", Source = "Website", Note = "Lead mới từ website", CreatedAt = today.AddDays(-10).AddHours(11) },
        new LeoEducation.Api.Models.CourseRegistration { RegistrationId = 7, FullName = "Bùi Anh G", Email = "student7@example.com", Phone = "0900000007", CourseId = 1, Status = "Đã hủy", Source = "Zalo", Note = "Phụ huynh chưa sắp xếp được thời gian", CreatedAt = today.AddDays(-20).AddHours(15) },
        new LeoEducation.Api.Models.CourseRegistration { RegistrationId = 8, StudentId = 4, FullName = "Hoàng Kim H", Email = "student8@example.com", Phone = "0900000008", CourseId = 2, Status = "Đã nhập học", Source = "Website", Note = "Đã xếp lớp IELTS", PaymentMode = "FullCourse", PaidAmount = 3200000, LastPaymentAt = today.AddMonths(-1).AddDays(3), TuitionNote = "Đã đóng trọn khóa", CreatedAt = today.AddMonths(-1).AddDays(3).AddHours(9) },
        new LeoEducation.Api.Models.CourseRegistration { RegistrationId = 9, FullName = "Mai Tuấn I", Email = "student9@example.com", Phone = "0900000009", CourseId = 1, Status = "Mới", Source = "Facebook", Note = "Cần kiểm tra đầu vào", CreatedAt = today.AddMonths(-1).AddDays(12).AddHours(17) },
        new LeoEducation.Api.Models.CourseRegistration { RegistrationId = 10, FullName = "Đỗ Thanh K", Email = "student10@example.com", Phone = "0900000010", CourseId = 3, Status = "Đã gọi", Source = "Giới thiệu", Note = "Đang cân nhắc học phí", CreatedAt = today.AddMonths(-2).AddDays(5).AddHours(10) },
        new LeoEducation.Api.Models.CourseRegistration { RegistrationId = 11, StudentId = 5, FullName = "Ngô Bảo L", Email = "student11@example.com", Phone = "0900000011", CourseId = 2, Status = "Đã nhập học", Source = "Website", Note = "Học thử đạt yêu cầu", PaymentMode = "FullCourse", PaidAmount = 1600000, LastPaymentAt = today.AddMonths(-5).AddDays(8), TuitionNote = "Đã cọc 50%", CreatedAt = today.AddMonths(-5).AddDays(8).AddHours(13) },
        new LeoEducation.Api.Models.CourseRegistration { RegistrationId = 12, FullName = "Cao Mỹ M", Email = "student12@example.com", Phone = "0900000012", CourseId = 1, Status = "Mới", Source = "Website", Note = "Đăng ký từ chiến dịch hè", CreatedAt = today.AddYears(-1).AddMonths(2).AddDays(4).AddHours(12) },
    };

    db.Users.Add(new LeoEducation.Api.Models.User
    {
        Id = 1,
        FullName = "Local Admin",
        Email = "admin@leo.local",
        Phone = "0900000000",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
        Status = "Active",
        CreatedAt = now,
        UpdatedAt = now
    });
    db.Instructors.AddRange(instructors);
    db.Subjects.AddRange(subjects);
    db.Courses.AddRange(courses);
    db.Students.AddRange(students);
    db.CourseRegistrations.AddRange(registrations);
    db.Classes.AddRange(classes);
    db.ClassStudents.AddRange(
        new LeoEducation.Api.Models.ClassStudent { ClassId = 1, RegistrationId = 1, CreatedAt = now },
        new LeoEducation.Api.Models.ClassStudent { ClassId = 3, RegistrationId = 5, CreatedAt = now },
        new LeoEducation.Api.Models.ClassStudent { ClassId = 2, RegistrationId = 8, CreatedAt = now },
        new LeoEducation.Api.Models.ClassStudent { ClassId = 2, RegistrationId = 11, CreatedAt = now }
    );
    db.ConsultationLogs.AddRange(
        new LeoEducation.Api.Models.ConsultationLog { ConsultationLogId = 1, RegistrationId = 3, ContactedAt = today.AddHours(9), Channel = "Zalo", StaffName = "Tư vấn viên A", Result = "Cần tư vấn thêm", Note = "Phụ huynh hỏi lịch buổi tối", CreatedAt = now },
        new LeoEducation.Api.Models.ConsultationLog { ConsultationLogId = 2, RegistrationId = 4, ContactedAt = today.AddDays(-3).AddHours(15), Channel = "Điện thoại", StaffName = "Tư vấn viên B", Result = "Hẹn gọi lại", Note = "Cuối tuần phụ huynh rảnh", CreatedAt = now },
        new LeoEducation.Api.Models.ConsultationLog { ConsultationLogId = 3, RegistrationId = 10, ContactedAt = today.AddMonths(-2).AddDays(5).AddHours(11), Channel = "Điện thoại", StaffName = "Tư vấn viên A", Result = "Đang cân nhắc", Note = "Gửi thêm thông tin học phí", CreatedAt = now }
    );

    await db.SaveChangesAsync();
}
