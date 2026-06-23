-- ============================================================
-- 06_triggers.sql
-- Purpose: Database-level business rule enforcement via triggers
-- Run AFTER: 02_schema.sql
-- Included: tr_Donations_AfterInsert
--   (updates DonationCampaigns.RaisedAmount automatically
--    whenever a Completed donation is inserted)
-- ============================================================

USE AlumniMS;
GO

-- ============================================================
-- Trigger: tr_Donations_AfterInsert
-- Fires:   AFTER INSERT on Donations
-- Purpose: Keep DonationCampaigns.RaisedAmount in sync.
--          When one or more Completed donations are inserted,
--          add their amounts to the campaign's RaisedAmount
--          and update the campaign's UpdatedAt timestamp.
-- ============================================================
CREATE OR ALTER TRIGGER tr_Donations_AfterInsert
ON Donations
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE c
    SET
        c.RaisedAmount = c.RaisedAmount + agg.TotalAdded,
        c.UpdatedAt    = SYSUTCDATETIME()
    FROM DonationCampaigns c
    INNER JOIN (
        SELECT CampaignId, SUM(Amount) AS TotalAdded
        FROM inserted
        WHERE Status = 'Completed'
        GROUP BY CampaignId
    ) AS agg ON c.CampaignId = agg.CampaignId;
END;
GO

PRINT 'Trigger tr_Donations_AfterInsert created successfully.';
GO
