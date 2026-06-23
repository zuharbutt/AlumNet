using AlumniManagementSystem.Data;
using AlumniManagementSystem.DTOs.Student;
using AlumniManagementSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlumniManagementSystem.Services;

public class StudentService(AppDbContext db)
{
    // ── GET /api/students/me ──────────────────────────────────────────────────
    public async Task<StudentProfileDto> GetMyProfileAsync(int userId)
    {
        var profile = await db.StudentProfiles
            .Include(s => s.User)
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.UserId == userId)
            ?? throw new KeyNotFoundException("Student profile not found.");

        return MapToDto(profile);
    }

    // ── PUT /api/students/me ──────────────────────────────────────────────────
    public async Task<StudentProfileDto> UpdateMyProfileAsync(int userId, UpdateStudentProfileDto dto)
    {
        // EnrollmentYear ≤ current year (FR-203 / DDD §4.6)
        if (dto.EnrollmentYear > DateTime.UtcNow.Year)
            throw new ArgumentException($"Enrollment year must be between 2000 and {DateTime.UtcNow.Year}.");

        // ExpectedGraduationYear > EnrollmentYear (FR-204)
        if (dto.ExpectedGraduationYear <= dto.EnrollmentYear)
            throw new ArgumentException("Expected graduation year must be greater than enrollment year.");

        // CGPA 0.00–4.00 (FR-203)
        if (dto.CGPA.HasValue && (dto.CGPA < 0 || dto.CGPA > 4))
            throw new ArgumentException("CGPA must be between 0.00 and 4.00.");

        // CurrentSemester 1–12
        if (dto.CurrentSemester.HasValue && (dto.CurrentSemester < 1 || dto.CurrentSemester > 12))
            throw new ArgumentException("Current semester must be between 1 and 12.");

        // Department must exist
        var dept = await db.Departments.FindAsync(dto.DepartmentId)
            ?? throw new ArgumentException("Invalid department.");

        var profile = await db.StudentProfiles
            .Include(s => s.User)
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.UserId == userId)
            ?? throw new KeyNotFoundException("Student profile not found.");

        profile.DepartmentId           = dto.DepartmentId;
        profile.EnrollmentYear         = dto.EnrollmentYear;
        profile.ExpectedGraduationYear = dto.ExpectedGraduationYear;
        profile.DegreeProgram          = dto.DegreeProgram;
        profile.CurrentSemester        = dto.CurrentSemester;
        profile.CGPA                   = dto.CGPA;
        profile.ShortBio               = dto.ShortBio;
        profile.UpdatedAt              = DateTime.UtcNow;

        await db.SaveChangesAsync();
        profile.Department = dept;
        return MapToDto(profile);
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    private static StudentProfileDto MapToDto(StudentProfile s) => new()
    {
        UserId                 = s.UserId,
        FullName               = s.User!.FullName,
        Email                  = s.User.Email,
        Phone                  = s.User.Phone,
        DepartmentId           = s.DepartmentId,
        DepartmentName         = s.Department?.Name ?? string.Empty,
        EnrollmentYear         = s.EnrollmentYear,
        ExpectedGraduationYear = s.ExpectedGraduationYear,
        DegreeProgram          = s.DegreeProgram,
        CurrentSemester        = s.CurrentSemester,
        CGPA                   = s.CGPA,
        ShortBio               = s.ShortBio,
    };
}
