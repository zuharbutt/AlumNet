-- ============================================================
-- 05_stored_procedures.sql
-- Purpose: Stored procedures for complex / transactional logic
-- Run AFTER: 04_views.sql
-- Included: sp_SearchAlumni, sp_RegisterForEvent
-- ============================================================

USE AlumniMS;
GO

-- ============================================================
-- SP: sp_SearchAlumni
-- Purpose: Alumni directory search with optional filters and
--          server-side pagination. Returns a TotalCount column
--          on every row so the caller knows total pages.
-- Parameters (all optional — omit to get all alumni):
--   @DepartmentId  INT           filter by department
--   @GradYearFrom  INT           graduation year range start
--   @GradYearTo    INT           graduation year range end
--   @Industry      NVARCHAR(100) partial match on Industry
--   @Location      NVARCHAR(150) partial match on WorkLocation
--   @Keyword       NVARCHAR(100) partial match on FullName OR CurrentCompany
--   @PageNumber    INT           1-based page number (default 1)
--   @PageSize      INT           rows per page (default 10, max 50)
-- ============================================================
CREATE OR ALTER PROCEDURE sp_SearchAlumni
    @DepartmentId   INT             = NULL,
    @GradYearFrom   INT             = NULL,
    @GradYearTo     INT             = NULL,
    @Industry       NVARCHAR(100)   = NULL,
    @Location       NVARCHAR(150)   = NULL,
    @Keyword        NVARCHAR(100)   = NULL,
    @PageNumber     INT             = 1,
    @PageSize       INT             = 10
AS
BEGIN
    SET NOCOUNT ON;

    -- Guard: sensible page size
    IF @PageSize > 50  SET @PageSize  = 50;
    IF @PageSize < 1   SET @PageSize  = 10;
    IF @PageNumber < 1 SET @PageNumber = 1;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    ;WITH Filtered AS (
        SELECT
            UserId,
            FullName,
            Email,
            Phone,
            AlumniProfileId,
            GraduationYear,
            DegreeProgram,
            DepartmentId,
            DepartmentName,
            CurrentCompany,
            JobTitle,
            Industry,
            WorkLocation,
            LinkedInUrl,
            ShortBio,
            RegisteredAt
        FROM vw_AlumniDirectory
        WHERE
            (@DepartmentId IS NULL OR DepartmentId    = @DepartmentId)
            AND (@GradYearFrom IS NULL OR GraduationYear >= @GradYearFrom)
            AND (@GradYearTo   IS NULL OR GraduationYear <= @GradYearTo)
            AND (@Industry     IS NULL OR Industry        LIKE '%' + @Industry  + '%')
            AND (@Location     IS NULL OR WorkLocation    LIKE '%' + @Location  + '%')
            AND (
                @Keyword IS NULL
                OR FullName        LIKE '%' + @Keyword + '%'
                OR CurrentCompany  LIKE '%' + @Keyword + '%'
            )
    )
    SELECT
        *,
        COUNT(*) OVER () AS TotalCount
    FROM Filtered
    ORDER BY GraduationYear DESC, FullName ASC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- ============================================================
-- SP: sp_RegisterForEvent
-- Purpose: Atomically register a user for an event and issue
--          a unique ticket. All validations run inside a
--          transaction; any error rolls back cleanly.
-- Parameters:
--   @EventId    INT              the event to register for
--   @UserId     INT              the user registering
-- Output:
--   @TicketCode NVARCHAR(50)     the generated ticket code
--   @ErrorMsg   NVARCHAR(500)    empty on success; populated on failure
-- ============================================================
CREATE OR ALTER PROCEDURE sp_RegisterForEvent
    @EventId    INT,
    @UserId     INT,
    @TicketCode NVARCHAR(50)  OUTPUT,
    @ErrorMsg   NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @TicketCode = NULL;
    SET @ErrorMsg   = NULL;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Load event details
        DECLARE @Capacity   INT,
                @Price      DECIMAL(10,2),
                @EventDate  DATETIME2,
                @Status     NVARCHAR(20);

        SELECT
            @Capacity  = Capacity,
            @Price     = TicketPrice,
            @EventDate = EventDate,
            @Status    = Status
        FROM Events
        WHERE EventId = @EventId;

        IF @Capacity IS NULL
        BEGIN
            SET @ErrorMsg = 'Event not found.';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @Status <> 'Scheduled'
        BEGIN
            SET @ErrorMsg = 'Event is not open for registration.';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @EventDate <= SYSUTCDATETIME()
        BEGIN
            SET @ErrorMsg = 'Registration has closed — event date has passed.';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- 2. Count active (non-cancelled) registrations
        DECLARE @Registered INT;
        SELECT @Registered = COUNT(*)
        FROM EventRegistrations
        WHERE EventId = @EventId AND CancelledAt IS NULL;

        IF @Registered >= @Capacity
        BEGIN
            SET @ErrorMsg = 'Event is full.';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- 3. Prevent duplicate registration
        IF EXISTS (
            SELECT 1
            FROM EventRegistrations
            WHERE EventId = @EventId AND UserId = @UserId AND CancelledAt IS NULL
        )
        BEGIN
            SET @ErrorMsg = 'You are already registered for this event.';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- 4. Insert registration
        INSERT INTO EventRegistrations (EventId, UserId)
        VALUES (@EventId, @UserId);

        DECLARE @RegId INT = SCOPE_IDENTITY();

        -- 5. Generate unique ticket code (TKT- prefix + GUID)
        SET @TicketCode = 'TKT-' + REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', '');

        -- 6. Insert ticket (payment is mocked — always Paid immediately)
        INSERT INTO Tickets (RegistrationId, TicketCode, Price, PaymentStatus)
        VALUES (@RegId, @TicketCode, @Price, 'Paid');

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @ErrorMsg = ERROR_MESSAGE();
        SET @TicketCode = NULL;
    END CATCH
END;
GO

PRINT 'Stored procedures created successfully.';
GO
