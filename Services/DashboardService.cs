using AlumniManagementSystem.Data;
using AlumniManagementSystem.DTOs.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace AlumniManagementSystem.Services;

public class DashboardService(AppDbContext db)
{
    // ── GET /api/dashboards/student ────────────────────────────────────────────
    // Returns counts scoped to the logged-in student (FR-804).
    public async Task<StudentDashboardDto> GetStudentDashboardAsync(int studentUserId)
    {
        var now = DateTime.UtcNow;

        var sentRequestsCount = await db.MentorshipRequests
            .Where(r => r.StudentUserId == studentUserId)
            .CountAsync();

        var acceptedMentorsCount = await db.MentorshipRequests
            .Where(r => r.StudentUserId == studentUserId && r.Status == "Accepted")
            .CountAsync();

        // Count distinct upcoming events the student is registered for
        var upcomingEventsCount = await db.Events
            .Where(e => e.Status == "Scheduled"
                     && e.EventDate > now
                     && e.Registrations.Any(er => er.UserId == studentUserId
                                               && er.CancelledAt == null))
            .CountAsync();

        var registeredEventsCount = await db.EventRegistrations
            .Where(er => er.UserId == studentUserId && er.CancelledAt == null)
            .CountAsync();

        return new StudentDashboardDto
        {
            SentRequestsCount     = sentRequestsCount,
            AcceptedMentorsCount  = acceptedMentorsCount,
            UpcomingEventsCount   = upcomingEventsCount,
            RegisteredEventsCount = registeredEventsCount,
        };
    }

    // ── GET /api/dashboards/alumni ─────────────────────────────────────────────
    // Returns counts scoped to the logged-in alumnus (FR-803).
    public async Task<AlumniDashboardDto> GetAlumniDashboardAsync(int alumniUserId)
    {
        var now = DateTime.UtcNow;

        var incomingRequestsCount = await db.MentorshipRequests
            .Where(r => r.AlumniUserId == alumniUserId && r.Status == "Pending")
            .CountAsync();

        var acceptedMenteeCount = await db.MentorshipRequests
            .Where(r => r.AlumniUserId == alumniUserId && r.Status == "Accepted")
            .CountAsync();

        var totalDonated = await db.Donations
            .Where(d => d.DonorUserId == alumniUserId && d.Status == "Completed")
            .SumAsync(d => (decimal?)d.Amount) ?? 0m;

        var upcomingEventsCount = await db.Events
            .Where(e => e.Status == "Scheduled"
                     && e.EventDate > now
                     && e.Registrations.Any(er => er.UserId == alumniUserId
                                               && er.CancelledAt == null))
            .CountAsync();

        return new AlumniDashboardDto
        {
            IncomingRequestsCount = incomingRequestsCount,
            AcceptedMenteeCount   = acceptedMenteeCount,
            TotalDonated          = totalDonated,
            UpcomingEventsCount   = upcomingEventsCount,
        };
    }

    // ── GET /api/dashboards/admin ──────────────────────────────────────────────
    // Mirrors sp_GetAdminDashboard stored procedure logic (FR-802, DDD §7).
    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        var now = DateTime.UtcNow;

        var totalAlumni = await db.Users
            .Where(u => u.Role == "Alumni" && u.IsActive)
            .CountAsync();

        var totalStudents = await db.Users
            .Where(u => u.Role == "Student" && u.IsActive)
            .CountAsync();

        var upcomingEvents = await db.Events
            .Where(e => e.Status == "Scheduled")
            .CountAsync();

        var activeCampaigns = await db.DonationCampaigns
            .Where(c => c.Status == "Active")
            .CountAsync();

        var totalRaised = await db.Donations
            .Where(d => d.Status == "Completed")
            .SumAsync(d => (decimal?)d.Amount) ?? 0m;

        var totalDonations = await db.Donations
            .Where(d => d.Status == "Completed")
            .CountAsync();

        var pendingMentorships = await db.MentorshipRequests
            .Where(r => r.Status == "Pending")
            .CountAsync();

        return new AdminDashboardDto
        {
            TotalAlumni        = totalAlumni,
            TotalStudents      = totalStudents,
            UpcomingEvents     = upcomingEvents,
            ActiveCampaigns    = activeCampaigns,
            TotalRaised        = totalRaised,
            TotalDonations     = totalDonations,
            PendingMentorships = pendingMentorships,
        };
    }

    // ── GET /api/admin/reports/mentorship ──────────────────────────────────────
    // Mirrors sp_GetMentorshipStats (DDD §7, FR-805).
    public async Task<MentorshipReportDto> GetMentorshipReportAsync(
        DateTime? fromDate, DateTime? toDate)
    {
        var query = db.MentorshipRequests.AsNoTracking();

        if (fromDate.HasValue) query = query.Where(r => r.RequestedAt >= fromDate.Value);
        if (toDate.HasValue)   query = query.Where(r => r.RequestedAt <= toDate.Value);

        var groups = await query
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        int Get(string s) => groups.FirstOrDefault(g => g.Status == s)?.Count ?? 0;

        return new MentorshipReportDto
        {
            TotalRequests = groups.Sum(g => g.Count),
            Pending       = Get("Pending"),
            Accepted      = Get("Accepted"),
            Rejected      = Get("Rejected"),
            Cancelled     = Get("Cancelled"),
        };
    }

    // ── GET /api/admin/reports/events ──────────────────────────────────────────
    // Mirrors sp_GetEventReport (DDD §7, FR-805).
    public async Task<EventReportDto> GetEventReportAsync()
    {
        var events = await db.Events
            .AsNoTracking()
            .Select(e => new EventReportItemDto
            {
                EventId         = e.EventId,
                Title           = e.Title,
                EventDate       = e.EventDate,
                Capacity        = e.Capacity,
                TotalRegistered = e.Registrations.Count(er => true),
                TotalAttended   = e.Registrations.Count(er => er.Attended),
                TotalCancelled  = e.Registrations.Count(er => er.CancelledAt != null),
                AttendanceRate  = e.Registrations.Count() == 0
                    ? 0m
                    : Math.Round(
                        (decimal)e.Registrations.Count(er => er.Attended) * 100m
                        / e.Registrations.Count(), 2),
            })
            .OrderByDescending(e => e.EventDate)
            .ToListAsync();

        return new EventReportDto
        {
            TotalEvents        = events.Count,
            TotalRegistrations = events.Sum(e => e.TotalRegistered),
            Events             = events,
        };
    }

    // ── GET /api/admin/users?role=X ────────────────────────────────────────────
    // SRS §11.6 — Admin sees all users filtered by role (FR-802).
    public async Task<AdminUserListDto> GetAdminUsersAsync(string role, int page, int pageSize)
    {
        var query = db.Users
            .AsNoTracking()
            .Where(u => u.Role == role && u.IsActive);

        var total = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserItemDto
            {
                UserId     = u.UserId,
                FullName   = u.FullName,
                Email      = u.Email,
                Department = role == "Alumni"
                    ? (u.AlumniProfile != null && u.AlumniProfile.Department != null
                        ? u.AlumniProfile.Department.Name : "—")
                    : (u.StudentProfile != null && u.StudentProfile.Department != null
                        ? u.StudentProfile.Department.Name : "—"),
                Status     = u.IsActive ? "Active" : "Inactive",
                JoinedAt   = u.CreatedAt,
            })
            .ToListAsync();

        return new AdminUserListDto
        {
            Users      = users,
            Total      = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        };
    }

    // ── GET /api/admin/mentorship ──────────────────────────────────────────────
    // SRS §11.6 — Admin oversight of all mentorship activity (DDD §6 vw_MentorshipOverview).
    public async Task<AdminMentorshipListDto> GetAdminMentorshipAsync(
        string? status, int page, int pageSize)
    {
        var query = db.MentorshipRequests
            .AsNoTracking()
            .Include(r => r.Student)
            .Include(r => r.Alumni);

        var filtered = string.IsNullOrEmpty(status)
            ? query
            : query.Where(r => r.Status == status);

        var total = await filtered.CountAsync();

        var items = await filtered
            .OrderByDescending(r => r.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new AdminMentorshipItemDto
            {
                RequestId    = r.RequestId,
                StudentName  = r.Student != null ? r.Student.FullName : "—",
                StudentEmail = r.Student != null ? r.Student.Email    : "—",
                AlumniName   = r.Alumni  != null ? r.Alumni.FullName  : "—",
                AlumniEmail  = r.Alumni  != null ? r.Alumni.Email     : "—",
                Status       = r.Status,
                RequestedAt  = r.RequestedAt,
                Message      = r.Message,
            })
            .ToListAsync();

        return new AdminMentorshipListDto
        {
            Requests   = items,
            Total      = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        };
    }

    // ── GET /api/admin/reports/donations ───────────────────────────────────────
    // Mirrors sp_GetDonationReport (DDD §7, FR-805).
    public async Task<DonationReportDto> GetDonationReportAsync(int? campaignId)
    {
        var query = db.DonationCampaigns.AsNoTracking();
        if (campaignId.HasValue) query = query.Where(c => c.CampaignId == campaignId.Value);

        var campaigns = await query
            .Select(c => new DonationReportItemDto
            {
                CampaignId      = c.CampaignId,
                Title           = c.Title,
                TargetAmount    = c.TargetAmount,
                RaisedAmount    = c.RaisedAmount,
                DonorCount      = c.Donations.Count(d => d.Status == "Completed"),
                AverageDonation = c.Donations.Any(d => d.Status == "Completed")
                    ? c.Donations.Where(d => d.Status == "Completed").Average(d => d.Amount)
                    : 0m,
                LargestDonation = c.Donations.Any(d => d.Status == "Completed")
                    ? c.Donations.Where(d => d.Status == "Completed").Max(d => d.Amount)
                    : 0m,
            })
            .ToListAsync();

        return new DonationReportDto
        {
            TotalCampaigns = campaigns.Count,
            TotalRaised    = campaigns.Sum(c => c.RaisedAmount),
            TotalDonors    = campaigns.Sum(c => c.DonorCount),
            Campaigns      = campaigns,
        };
    }
}
