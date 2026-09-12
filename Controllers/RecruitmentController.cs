using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;
using LeoEducation.Api.Data;
using LeoEducation.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LeoEducation.Api.Controllers;

[ApiController, Route("api/recruitment"), Authorize]
public class RecruitmentController(ApplicationDbContext db) : ControllerBase
{
    private static readonly string[] States = ["new", "reviewing", "interview", "hired", "rejected"];
    private IQueryable<RecruitmentJob> OpenJobs() => db.RecruitmentJobs.Where(j => j.IsActive && (j.ClosesAt == null || j.ClosesAt > DateTime.UtcNow));
    private ObjectResult Failure(int status, string code) => StatusCode(status, new { success = false, code });

    [HttpGet("jobs"), AllowAnonymous]
    public async Task<IActionResult> Jobs() => Ok(new { success = true, data = await OpenJobs().AsNoTracking().OrderByDescending(j => j.Id).ToListAsync() });

    [HttpGet("jobs/{id:int}"), AllowAnonymous]
    public async Task<IActionResult> Job(int id)
    {
        var job = await OpenJobs().AsNoTracking().FirstOrDefaultAsync(j => j.Id == id);
        return job == null ? Failure(404, "NOT_FOUND") : Ok(new { success = true, data = job });
    }

    [HttpPost("jobs/{id:int}/applications"), AllowAnonymous, EnableRateLimiting("recruitment")]
    public async Task<IActionResult> Apply(int id, ApplicationInput input)
    {
        if (!input.Consent || !new[] { "vi", "en", "zh" }.Contains(input.Locale)
            || !Uri.TryCreate(input.ResumeUrl, UriKind.Absolute, out var resume) || resume.Scheme != "https" || resume.UserInfo.Length > 0
            || !Regex.IsMatch(input.Phone, @"^[+\d\s().-]{7,30}$")) return Failure(400, "INVALID_INPUT");
        // Serializable transactions keep closing a role and submitting to it consistent.
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable) : null;
        if (!await OpenJobs().AnyAsync(j => j.Id == id)) return Failure(410, "CLOSED");
        var email = input.Email.Trim().ToLowerInvariant();
        if (await db.JobApplications.AnyAsync(a => a.JobId == id && a.Email == email)) return Failure(409, "DUPLICATE");
        var application = new JobApplication { JobId = id, FullName = input.FullName.Trim(), Email = email,
            Phone = input.Phone.Trim(), ResumeUrl = input.ResumeUrl.Trim(), CoverLetter = input.CoverLetter.Trim(), Locale = input.Locale };
        db.JobApplications.Add(application);
        try
        {
            await db.SaveChangesAsync();
            if (transaction != null) await transaction.CommitAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" }) { return Failure(409, "DUPLICATE"); }
        return StatusCode(201, new { success = true, data = new { application.Id } });
    }

    [HttpGet("admin/jobs")]
    public async Task<IActionResult> AdminJobs() => Ok(new { success = true, data = await db.RecruitmentJobs.AsNoTracking().OrderByDescending(j => j.Id).ToListAsync() });

    [HttpPost("admin/jobs")]
    public async Task<IActionResult> Create(JobInput input)
    {
        if (!Valid(input)) return Failure(400, "INVALID_INPUT");
        var job = new RecruitmentJob(); Set(job, input); db.Add(job); await db.SaveChangesAsync();
        return StatusCode(201, new { success = true, data = job });
    }

    [HttpPut("admin/jobs/{id:int}")]
    public async Task<IActionResult> Update(int id, JobInput input)
    {
        if (!Valid(input)) return Failure(400, "INVALID_INPUT");
        var job = await db.RecruitmentJobs.FindAsync(id);
        if (job == null) return Failure(404, "NOT_FOUND");
        Set(job, input); await db.SaveChangesAsync();
        return Ok(new { success = true, data = job });
    }

    [HttpGet("admin/applications")]
    public async Task<IActionResult> Applications(int page = 1, int? jobId = null, string? status = null)
    {
        Response.Headers.CacheControl = "no-store";
        page = Math.Clamp(page, 1, 100000);
        var query = db.JobApplications.AsNoTracking();
        if (jobId != null) query = query.Where(a => a.JobId == jobId);
        if (!string.IsNullOrEmpty(status)) query = query.Where(a => a.Status == status);
        return Ok(new { success = true, total = await query.CountAsync(), page,
            data = await query.OrderByDescending(a => a.Id).Skip((page - 1) * 50).Take(50).ToListAsync() });
    }

    [HttpPatch("admin/applications/{id:int}")]
    public async Task<IActionResult> UpdateApplication(int id, StatusInput input)
    {
        if (!States.Contains(input.Status)) return Failure(400, "INVALID_INPUT");
        var application = await db.JobApplications.FindAsync(id);
        if (application == null) return Failure(404, "NOT_FOUND");
        application.Status = input.Status; await db.SaveChangesAsync();
        return Ok(new { success = true });
    }

    private static void Set(RecruitmentJob job, JobInput input)
    {
        job.TranslationsJson = input.Translations.GetRawText(); job.Department = input.Department;
        job.EmploymentType = input.EmploymentType; job.WorkMode = input.WorkMode;
        job.ClosesAt = input.ClosesAt?.UtcDateTime; job.IsActive = input.IsActive;
    }
    private static bool Valid(JobInput input)
    {
        if (!new[] { "teaching", "academic", "operations" }.Contains(input.Department)
            || !new[] { "full-time", "part-time", "contract" }.Contains(input.EmploymentType)
            || !new[] { "remote", "onsite", "hybrid" }.Contains(input.WorkMode)
            || input.Translations.ValueKind != JsonValueKind.Object) return false;
        foreach (var locale in new[] { "vi", "en", "zh" })
        {
            if (!input.Translations.TryGetProperty(locale, out var copy) || copy.ValueKind != JsonValueKind.Object) return false;
            foreach (var field in new[] { "title", "location", "salary", "description", "requirements", "benefits" })
                if (!copy.TryGetProperty(field, out var value) || value.ValueKind != JsonValueKind.String
                    || string.IsNullOrWhiteSpace(value.GetString()) || value.GetString()!.Length > (field is "title" or "location" or "salary" ? 300 : 10000)) return false;
        }
        return true;
    }
}

public class JobInput
{
    public JsonElement Translations { get; set; }
    [Required] public string Department { get; set; } = "";
    [Required] public string EmploymentType { get; set; } = "";
    [Required] public string WorkMode { get; set; } = "";
    public DateTimeOffset? ClosesAt { get; set; }
    public bool IsActive { get; set; }
}
public class ApplicationInput
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required, StringLength(30)] public string Phone { get; set; } = "";
    [Required, StringLength(2000)] public string ResumeUrl { get; set; } = "";
    [StringLength(5000)] public string CoverLetter { get; set; } = "";
    public string Locale { get; set; } = "vi";
    public bool Consent { get; set; }
}
public record StatusInput(string Status);
