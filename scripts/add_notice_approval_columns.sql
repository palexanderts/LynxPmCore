-- Migration: Add rejection reason column to LYNX_PM_AVISO
-- Run this script once against the Oracle database before deploying the approval workflow feature.

ALTER TABLE LYNX_PM_AVISO ADD (
    REJECTION_REASON VARCHAR2(500)
);

-- Backfill: existing approved notices keep IS_APPROVED=1; rejected ones have REJECTION_REASON set.
-- No data migration needed — REJECTION_REASON defaults to NULL (Pending state).
