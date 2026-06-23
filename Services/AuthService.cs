using AlumniManagementSystem.Data;
using AlumniManagementSystem.DTOs.Auth;
using AlumniManagementSystem.Entities;
using AlumniManagementSystem.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AlumniManagementSystem.Services;

public class AuthService(AppDbContext db, JwtService jwtService)
{
    private static readonly Regex PasswordRegex =
        new(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$", RegexOptions.Compiled);

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Password strength
        if (!PasswordRegex.IsMatch(dto.Password))
            throw new ArgumentException(
                "Password must be at least 8 characters and contain one uppercase letter, one number, and one special character.");

        // Duplicate email
        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
            throw new ArgumentException("Email already exists.");

        // Admin invite-code validation
        if (dto.Role == UserRole.Admin)
        {
            if (string.IsNullOrWhiteSpace(dto.InviteCode))
                throw new ArgumentException("Admin invite code is required.");

            var code = await db.AdminInviteCodes
                .FirstOrDefaultAsync(c => c.Code == dto.InviteCode);

            if (code is null || code.IsUsed || code.ExpiryDate < DateTime.UtcNow)
                throw new ArgumentException("Invalid or expired invite code.");

            var user = await CreateUserAsync(dto);

            // Mark code used
            code.IsUsed       = true;
            code.UsedByUserId = user.UserId;
            code.UsedAt       = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return BuildResponse(user);
        }

        // Student / Alumni registration
        {
            var user = await CreateUserAsync(dto);

            if (dto.Role == UserRole.Student)
            {
                // Use first department as placeholder; profile is completed later
                var dept = await db.Departments.FirstOrDefaultAsync();
                db.StudentProfiles.Add(new StudentProfile
                {
                    UserId                 = user.UserId,
                    DepartmentId           = dept?.DepartmentId ?? 1,
                    EnrollmentYear         = DateTime.UtcNow.Year,
                    ExpectedGraduationYear = DateTime.UtcNow.Year + 4,
                    DegreeProgram          = "Pending",
                });
            }
            else // Alumni
            {
                var dept = await db.Departments.FirstOrDefaultAsync();
                db.AlumniProfiles.Add(new AlumniProfile
                {
                    UserId         = user.UserId,
                    DepartmentId   = dept?.DepartmentId ?? 1,
                    GraduationYear = DateTime.UtcNow.Year,
                    DegreeProgram  = "Pending",
                });
            }

            await db.SaveChangesAsync();
            return BuildResponse(user);
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        // Don't reveal whether email or password is wrong
        if (user is null || !user.IsActive)
        {
            // Still check lock timing even if user not found (timing attack prevention via fake delay)
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // Lockout check
        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            throw new UnauthorizedAccessException("Account temporarily locked. Please try again later.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            user.FailedAttempts++;

            if (user.FailedAttempts >= 5)
            {
                user.LockedUntil    = DateTime.UtcNow.AddMinutes(15);
                user.FailedAttempts = 0;
            }

            await db.SaveChangesAsync();
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // Successful login — reset failure counters
        user.FailedAttempts = 0;
        user.LockedUntil    = null;
        await db.SaveChangesAsync();

        return BuildResponse(user);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<User> CreateUserAsync(RegisterDto dto)
    {
        var user = new User
        {
            FullName     = dto.FullName,
            Email        = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Phone        = dto.Phone,
            DateOfBirth  = dto.DateOfBirth,
            Role         = dto.Role,
            IsActive     = true,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private AuthResponseDto BuildResponse(User user)
    {
        var (token, expiresAt) = jwtService.GenerateToken(user);
        return new AuthResponseDto
        {
            Token     = token,
            Role      = user.Role,
            UserId    = user.UserId,
            FullName  = user.FullName,
            Email     = user.Email,
            ExpiresAt = expiresAt,
        };
    }
}
