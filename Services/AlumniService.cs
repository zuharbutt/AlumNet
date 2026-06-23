using AlumniManagementSystem.Data;
using AlumniManagementSystem.DTOs.Alumni;
using AlumniManagementSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlumniManagementSystem.Services;

public class AlumniService(AppDbContext db)
{
    // ── GET /api/alumni/me ────────────────────────────────────────────────────
    public async Task<AlumniProfileDto> GetMyProfileAsync(int userId)
    {
        var profile = await db.AlumniProfiles
            .Include(a => a.User)
            .Include(a => a.Department)
            .FirstOrDefaultAsync(a => a.UserId == userId)
            ?? throw new KeyNotFoundException("Alumni profile not found.");

        return MapToProfileDto(profile);
    }

    // ── PUT /api/alumni/me ────────────────────────────────────────────────────
    public async Task<AlumniProfileDto> UpdateMyProfileAsync(int userId, UpdateAlumniProfileDto dto)
    {
        // Graduation year ≤ current year (DDD §4.5, FR-202)
        if (dto.GraduationYear > DateTime.UtcNow.Year)
            throw new ArgumentException($"Graduation year must be between 1950 and {DateTime.UtcNow.Year}.");

        // Department must exist
        var dept = await db.Departments.FindAsync(dto.DepartmentId)
            ?? throw new ArgumentException("Invalid department.");

        // LinkedIn URL validation (DDD §4.5: must contain linkedin.com)
        if (!string.IsNullOrWhiteSpace(dto.LinkedInUrl) &&
            !dto.LinkedInUrl.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("LinkedIn URL must contain 'linkedin.com'.");

        var profile = await db.AlumniProfiles
            .Include(a => a.User)
            .Include(a => a.Department)
            .FirstOrDefaultAsync(a => a.UserId == userId)
            ?? throw new KeyNotFoundException("Alumni profile not found.");

        profile.DepartmentId   = dto.DepartmentId;
        profile.GraduationYear = dto.GraduationYear;
        profile.DegreeProgram  = dto.DegreeProgram;
        profile.CurrentCompany = dto.CurrentCompany;
        profile.JobTitle       = dto.JobTitle;
        profile.Industry       = dto.Industry;
        profile.WorkLocation   = dto.WorkLocation;
        profile.LinkedInUrl    = dto.LinkedInUrl;
        profile.ShortBio       = dto.ShortBio;
        profile.UpdatedAt      = DateTime.UtcNow;

        await db.SaveChangesAsync();
        profile.Department = dept;
        return MapToProfileDto(profile);
    }

    // ── GET /api/alumni (directory) ───────────────────────────────────────────
    public async Task<AlumniDirectoryResultDto> GetDirectoryAsync(
        int? departmentId, int? yearFrom, int? yearTo,
        string? industry, string? location, string? keyword,
        int page, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page     = Math.Max(page, 1);

        var query = db.AlumniProfiles
            .Include(a => a.User)
            .Include(a => a.Department)
            .Where(a => a.User!.IsActive)
            .AsQueryable();

        if (departmentId.HasValue)
            query = query.Where(a => a.DepartmentId == departmentId.Value);

        if (yearFrom.HasValue)
            query = query.Where(a => a.GraduationYear >= yearFrom.Value);

        if (yearTo.HasValue)
            query = query.Where(a => a.GraduationYear <= yearTo.Value);

        if (!string.IsNullOrWhiteSpace(industry))
            query = query.Where(a => a.Industry != null && a.Industry.Contains(industry));

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(a => a.WorkLocation != null && a.WorkLocation.Contains(location));

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(a =>
                a.User!.FullName.Contains(keyword) ||
                (a.CurrentCompany != null && a.CurrentCompany.Contains(keyword)));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.GraduationYear)
            .ThenBy(a => a.User!.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AlumniDirectoryItemDto
            {
                UserId         = a.UserId,
                FullName       = a.User!.FullName,
                GraduationYear = a.GraduationYear,
                DepartmentName = a.Department!.Name,
                CurrentCompany = a.CurrentCompany,
                JobTitle       = a.JobTitle,
                Industry       = a.Industry,
                WorkLocation   = a.WorkLocation,
            })
            .ToListAsync();

        return new AlumniDirectoryResultDto
        {
            Items      = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize   = pageSize,
        };
    }

    // ── GET /api/alumni/{id} ──────────────────────────────────────────────────
    public async Task<AlumniDetailDto> GetByIdAsync(int alumniUserId, int requestingUserId)
    {
        // Uses vw_AlumniDirectory logic (User + AlumniProfile + Department join)
        var profile = await db.AlumniProfiles
            .Include(a => a.User)
            .Include(a => a.Department)
            .FirstOrDefaultAsync(a => a.UserId == alumniUserId && a.User!.IsActive)
            ?? throw new KeyNotFoundException("Alumni not found.");

        // Contact visible if accepted mentorship exists between requester and this alumnus
        var contactVisible = await db.MentorshipRequests.AnyAsync(m =>
            m.StudentUserId == requestingUserId &&
            m.AlumniUserId  == alumniUserId &&
            m.Status        == "Accepted");

        return new AlumniDetailDto
        {
            UserId         = profile.UserId,
            FullName       = profile.User!.FullName,
            Email          = contactVisible ? profile.User.Email : null,
            Phone          = contactVisible ? profile.User.Phone  : null,
            ContactVisible = contactVisible,
            GraduationYear = profile.GraduationYear,
            DepartmentName = profile.Department!.Name,
            DegreeProgram  = profile.DegreeProgram,
            CurrentCompany = profile.CurrentCompany,
            JobTitle       = profile.JobTitle,
            Industry       = profile.Industry,
            WorkLocation   = profile.WorkLocation,
            LinkedInUrl    = profile.LinkedInUrl,
            ShortBio       = profile.ShortBio,
        };
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    private static AlumniProfileDto MapToProfileDto(AlumniProfile a) => new()
    {
        UserId         = a.UserId,
        FullName       = a.User!.FullName,
        Email          = a.User.Email,
        Phone          = a.User.Phone,
        DepartmentId   = a.DepartmentId,
        DepartmentName = a.Department?.Name ?? string.Empty,
        GraduationYear = a.GraduationYear,
        DegreeProgram  = a.DegreeProgram,
        CurrentCompany = a.CurrentCompany,
        JobTitle       = a.JobTitle,
        Industry       = a.Industry,
        WorkLocation   = a.WorkLocation,
        LinkedInUrl    = a.LinkedInUrl,
        ShortBio       = a.ShortBio,
    };
}
