using LeoEducation.Api.Models;

namespace LeoEducation.Api.Utils;

public static class TuitionCalculator
{
    public const string Monthly = "Monthly";
    public const string FullCourse = "FullCourse";

    public static int GetDurationMonths(Course course)
    {
        if (!course.StartDate.HasValue || !course.EndDate.HasValue || course.EndDate.Value < course.StartDate.Value)
            return 1;

        var start = course.StartDate.Value;
        var end = course.EndDate.Value;
        var months = ((end.Year - start.Year) * 12) + end.Month - start.Month + 1;
        return Math.Max(1, months);
    }

    public static decimal GetTotalFee(Course course)
    {
        var price = course.Price ?? 0m;
        return course.BillingType == Monthly ? price * GetDurationMonths(course) : price;
    }

    public static string GetPaymentMode(CourseRegistration registration)
        => string.IsNullOrWhiteSpace(registration.PaymentMode)
            ? registration.Course.BillingType
            : registration.PaymentMode;

    public static string GetTuitionStatus(CourseRegistration registration)
    {
        var totalFee = GetTotalFee(registration.Course);
        if (registration.PaidAmount <= 0) return "Chưa đóng";
        if (totalFee <= 0 || registration.PaidAmount >= totalFee) return "Đã đóng đủ";
        return GetPaymentMode(registration) == Monthly ? "Đang đóng tháng" : "Còn thiếu";
    }

    public static DateTime? GetNextDueDate(CourseRegistration registration)
    {
        var totalFee = GetTotalFee(registration.Course);
        if (totalFee <= 0 || registration.PaidAmount >= totalFee) return null;

        var startDate = registration.Course.StartDate?.Date ?? registration.CreatedAt.Date;
        if (GetPaymentMode(registration) != Monthly) return startDate;

        var monthlyFee = registration.Course.Price ?? 0m;
        if (monthlyFee <= 0) return startDate;

        var paidMonths = (int)Math.Floor(registration.PaidAmount / monthlyFee);
        var durationMonths = GetDurationMonths(registration.Course);
        if (paidMonths >= durationMonths) return null;

        return startDate.AddMonths(Math.Max(0, paidMonths));
    }

    public static bool IsTuitionDue(CourseRegistration registration, DateTime? now = null)
    {
        var dueDate = GetNextDueDate(registration);
        if (!dueDate.HasValue) return false;
        return dueDate.Value.Date <= (now ?? DateTime.UtcNow).Date;
    }

    public static string GetDueStatus(CourseRegistration registration, DateTime? now = null)
    {
        if (GetTuitionStatus(registration) == "Đã đóng đủ") return "Đã đủ";
        return IsTuitionDue(registration, now) ? "Đến hạn thu" : "Chưa đến hạn";
    }
}
