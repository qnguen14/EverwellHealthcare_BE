-- Manual migration script for adding Google Meet columns to Appointment table
-- Run this in your Supabase SQL Editor

-- Check current schema
DO $$
BEGIN
    -- Add google_meet_link column
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'EverWellDB_v2.5' 
        AND table_name = 'Appointment' 
        AND column_name = 'google_meet_link'
    ) THEN
        ALTER TABLE "EverWellDB_v2.5"."Appointment" 
        ADD COLUMN "google_meet_link" text;
    END IF;

    -- Add google_event_id column
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'EverWellDB_v2.5' 
        AND table_name = 'Appointment' 
        AND column_name = 'google_event_id'
    ) THEN
        ALTER TABLE "EverWellDB_v2.5"."Appointment" 
        ADD COLUMN "google_event_id" text;
    END IF;

    -- Add meeting_id column
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'EverWellDB_v2.5' 
        AND table_name = 'Appointment' 
        AND column_name = 'meeting_id'
    ) THEN
        ALTER TABLE "EverWellDB_v2.5"."Appointment" 
        ADD COLUMN "meeting_id" text;
    END IF;

    -- Add is_virtual column
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'EverWellDB_v2.5' 
        AND table_name = 'Appointment' 
        AND column_name = 'is_virtual'
    ) THEN
        ALTER TABLE "EverWellDB_v2.5"."Appointment" 
        ADD COLUMN "is_virtual" boolean NOT NULL DEFAULT true;
    END IF;
END $$;

-- Verify columns were added
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns 
WHERE table_schema = 'EverWellDB_v2.5'
AND table_name = 'Appointment'
AND column_name IN ('google_meet_link', 'google_event_id', 'meeting_id', 'is_virtual')
ORDER BY column_name; 