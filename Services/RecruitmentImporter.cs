using System.Text.Json;
using LeoEducation.Api.Data;
using LeoEducation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LeoEducation.Api.Services;

public static class RecruitmentImporter
{
    public static async Task<int> ImportAsync(ApplicationDbContext db, string file)
    {
        if (!db.Database.IsRelational()) throw new InvalidOperationException("Recruitment import requires a persistent database.");
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(file));
        var jobs = document.RootElement.EnumerateArray();
        var existing = (await db.RecruitmentJobs.ToListAsync())
            .Select(j => j.Translations.GetProperty("vi").GetProperty("title").GetString()).ToHashSet();
        var count = 0;
        foreach (var item in jobs)
        {
            var translations = item.GetProperty("translations");
            var title = translations.GetProperty("vi").GetProperty("title").GetString();
            if (!existing.Add(title)) continue;
            db.RecruitmentJobs.Add(new RecruitmentJob
            {
                TranslationsJson = translations.GetRawText(), Department = item.GetProperty("department").GetString()!,
                EmploymentType = item.GetProperty("employmentType").GetString()!, WorkMode = item.GetProperty("workMode").GetString()!,
                IsActive = item.GetProperty("isActive").GetBoolean(),
                ClosesAt = item.TryGetProperty("closesAt", out var closes) && closes.ValueKind != JsonValueKind.Null ? closes.GetDateTime() : null,
            });
            count++;
        }
        await db.SaveChangesAsync();
        return count;
    }
}
