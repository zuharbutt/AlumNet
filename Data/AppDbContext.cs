using AlumniManagementSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlumniManagementSystem.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User>               Users               => Set<User>();
    public DbSet<Department>         Departments         => Set<Department>();
    public DbSet<AdminInviteCode>    AdminInviteCodes    => Set<AdminInviteCode>();
    public DbSet<AlumniProfile>      AlumniProfiles      => Set<AlumniProfile>();
    public DbSet<StudentProfile>     StudentProfiles     => Set<StudentProfile>();
    public DbSet<MentorshipRequest>  MentorshipRequests  => Set<MentorshipRequest>();
    public DbSet<Event>              Events              => Set<Event>();
    public DbSet<EventRegistration>  EventRegistrations  => Set<EventRegistration>();
    public DbSet<Ticket>             Tickets             => Set<Ticket>();
    public DbSet<DonationCampaign>   DonationCampaigns   => Set<DonationCampaign>();
    public DbSet<Donation>           Donations           => Set<Donation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Users ────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(u => u.UserId);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            e.Property(u => u.Email).HasMaxLength(150).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(u => u.Phone).HasMaxLength(20);
            e.Property(u => u.Role).HasMaxLength(20).IsRequired();
            e.Property(u => u.IsActive).HasDefaultValue(true);
            e.Property(u => u.FailedAttempts).HasDefaultValue(0);
            e.Property(u => u.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // ── Departments ──────────────────────────────────────────────────────
        modelBuilder.Entity<Department>(e =>
        {
            e.ToTable("Departments");
            e.HasKey(d => d.DepartmentId);
            e.HasIndex(d => d.Name).IsUnique();
            e.Property(d => d.Name).HasMaxLength(100).IsRequired();
            e.Property(d => d.Description).HasMaxLength(500);
            e.Property(d => d.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // ── AdminInviteCodes ─────────────────────────────────────────────────
        modelBuilder.Entity<AdminInviteCode>(e =>
        {
            e.ToTable("AdminInviteCodes");
            e.HasKey(a => a.CodeId);
            e.HasIndex(a => a.Code).IsUnique();
            e.Property(a => a.Code).HasMaxLength(50).IsRequired();
            e.Property(a => a.IsUsed).HasDefaultValue(false);
            e.Property(a => a.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(a => a.CreatedBy)
             .WithMany()
             .HasForeignKey(a => a.CreatedByUserId)
             .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(a => a.UsedBy)
             .WithMany()
             .HasForeignKey(a => a.UsedByUserId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── AlumniProfiles ───────────────────────────────────────────────────
        modelBuilder.Entity<AlumniProfile>(e =>
        {
            e.ToTable("AlumniProfiles");
            e.HasKey(a => a.AlumniProfileId);
            e.HasIndex(a => a.UserId).IsUnique();
            e.Property(a => a.DegreeProgram).HasMaxLength(100).IsRequired();
            e.Property(a => a.CurrentCompany).HasMaxLength(150);
            e.Property(a => a.JobTitle).HasMaxLength(100);
            e.Property(a => a.Industry).HasMaxLength(100);
            e.Property(a => a.WorkLocation).HasMaxLength(150);
            e.Property(a => a.LinkedInUrl).HasMaxLength(300);
            e.Property(a => a.ShortBio).HasMaxLength(1000);
            e.Property(a => a.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(a => a.User)
             .WithOne(u => u.AlumniProfile)
             .HasForeignKey<AlumniProfile>(a => a.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.Department)
             .WithMany(d => d.AlumniProfiles)
             .HasForeignKey(a => a.DepartmentId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── StudentProfiles ──────────────────────────────────────────────────
        modelBuilder.Entity<StudentProfile>(e =>
        {
            e.ToTable("StudentProfiles");
            e.HasKey(s => s.StudentProfileId);
            e.HasIndex(s => s.UserId).IsUnique();
            e.Property(s => s.DegreeProgram).HasMaxLength(100).IsRequired();
            e.Property(s => s.ShortBio).HasMaxLength(1000);
            e.Property(s => s.CGPA).HasColumnType("decimal(3,2)");
            e.Property(s => s.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(s => s.User)
             .WithOne(u => u.StudentProfile)
             .HasForeignKey<StudentProfile>(s => s.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(s => s.Department)
             .WithMany(d => d.StudentProfiles)
             .HasForeignKey(s => s.DepartmentId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── MentorshipRequests ───────────────────────────────────────────────
        modelBuilder.Entity<MentorshipRequest>(e =>
        {
            e.ToTable("MentorshipRequests");
            e.HasKey(m => m.RequestId);
            e.Property(m => m.Message).HasMaxLength(500).IsRequired();
            e.Property(m => m.Status).HasMaxLength(20).HasDefaultValue("Pending");
            e.Property(m => m.ResponseMessage).HasMaxLength(500);
            e.Property(m => m.RequestedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(m => m.Student)
             .WithMany()
             .HasForeignKey(m => m.StudentUserId)
             .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(m => m.Alumni)
             .WithMany()
             .HasForeignKey(m => m.AlumniUserId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── Events ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Event>(e =>
        {
            e.ToTable("Events");
            e.HasKey(ev => ev.EventId);
            e.Property(ev => ev.Title).HasMaxLength(200).IsRequired();
            e.Property(ev => ev.Description).HasMaxLength(2000).IsRequired();
            e.Property(ev => ev.Category).HasMaxLength(50).IsRequired();
            e.Property(ev => ev.Location).HasMaxLength(300).IsRequired();
            e.Property(ev => ev.TicketPrice).HasColumnType("decimal(10,2)").HasDefaultValue(0m);
            e.Property(ev => ev.Status).HasMaxLength(20).HasDefaultValue("Scheduled");
            e.Property(ev => ev.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(ev => ev.CreatedBy)
             .WithMany()
             .HasForeignKey(ev => ev.CreatedByUserId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── EventRegistrations ───────────────────────────────────────────────
        modelBuilder.Entity<EventRegistration>(e =>
        {
            e.ToTable("EventRegistrations");
            e.HasKey(er => er.RegistrationId);
            e.HasIndex(er => new { er.EventId, er.UserId }).IsUnique();
            e.Property(er => er.Attended).HasDefaultValue(false);
            e.Property(er => er.RegisteredAt).HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(er => er.Event)
             .WithMany(ev => ev.Registrations)
             .HasForeignKey(er => er.EventId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(er => er.User)
             .WithMany()
             .HasForeignKey(er => er.UserId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── Tickets ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Ticket>(e =>
        {
            e.ToTable("Tickets");
            e.HasKey(t => t.TicketId);
            e.HasIndex(t => t.RegistrationId).IsUnique();
            e.HasIndex(t => t.TicketCode).IsUnique();
            e.Property(t => t.TicketCode).HasMaxLength(50).IsRequired();
            e.Property(t => t.Price).HasColumnType("decimal(10,2)");
            e.Property(t => t.PaymentStatus).HasMaxLength(20).HasDefaultValue("Paid");
            e.Property(t => t.IssuedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(t => t.Registration)
             .WithOne(er => er.Ticket)
             .HasForeignKey<Ticket>(t => t.RegistrationId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── DonationCampaigns ────────────────────────────────────────────────
        modelBuilder.Entity<DonationCampaign>(e =>
        {
            e.ToTable("DonationCampaigns");
            e.HasKey(c => c.CampaignId);
            e.Property(c => c.Title).HasMaxLength(200).IsRequired();
            e.Property(c => c.Description).HasMaxLength(2000).IsRequired();
            e.Property(c => c.TargetAmount).HasColumnType("decimal(14,2)");
            e.Property(c => c.RaisedAmount).HasColumnType("decimal(14,2)").HasDefaultValue(0m);
            e.Property(c => c.Status).HasMaxLength(20).HasDefaultValue("Active");
            e.Property(c => c.StartDate).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(c => c.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(c => c.CreatedBy)
             .WithMany()
             .HasForeignKey(c => c.CreatedByUserId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── Donations ────────────────────────────────────────────────────────
        modelBuilder.Entity<Donation>(e =>
        {
            e.ToTable("Donations", tb => tb.HasTrigger("tr_Donations_AfterInsert"));
            e.HasKey(d => d.DonationId);
            e.Property(d => d.Amount).HasColumnType("decimal(14,2)");
            e.Property(d => d.IsAnonymous).HasDefaultValue(false);
            e.Property(d => d.Status).HasMaxLength(20).HasDefaultValue("Completed");
            e.Property(d => d.DonatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(d => d.Campaign)
             .WithMany(c => c.Donations)
             .HasForeignKey(d => d.CampaignId)
             .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(d => d.Donor)
             .WithMany()
             .HasForeignKey(d => d.DonorUserId)
             .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
