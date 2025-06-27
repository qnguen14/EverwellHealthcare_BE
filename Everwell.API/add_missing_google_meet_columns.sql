-- Add missing Google Meet columns to existing Appointment table
-- Run this in your Supabase SQL Editor

-- Add google_event_id column (if not exists)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'EverWellDB_v2.5' 
        AND table_name = 'Appointment' 
        AND column_name = 'google_event_id'
    ) THEN
        ALTER TABLE "EverWellDB_v2.5"."Appointment" 
        ADD COLUMN "google_event_id" text;
        RAISE NOTICE 'Added google_event_id column';
    ELSE
        RAISE NOTICE 'google_event_id column already exists';
    END IF;
END $$;

-- Add meeting_id column (if not exists)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'EverWellDB_v2.5' 
        AND table_name = 'Appointment' 
        AND column_name = 'meeting_id'
    ) THEN
        ALTER TABLE "EverWellDB_v2.5"."Appointment" 
        ADD COLUMN "meeting_id" text;
        RAISE NOTICE 'Added meeting_id column';
    ELSE
        RAISE NOTICE 'meeting_id column already exists';
    END IF;
END $$;

-- Add is_virtual column (if not exists)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'EverWellDB_v2.5' 
        AND table_name = 'Appointment' 
        AND column_name = 'is_virtual'
    ) THEN
        ALTER TABLE "EverWellDB_v2.5"."Appointment" 
        ADD COLUMN "is_virtual" boolean NOT NULL DEFAULT true;
        RAISE NOTICE 'Added is_virtual column';
    ELSE
        RAISE NOTICE 'is_virtual column already exists';
    END IF;
END $$;

-- Verify all Google Meet columns exist
SELECT 
    column_name, 
    data_type, 
    character_maximum_length,
    is_nullable, 
    column_default
FROM information_schema.columns 
WHERE table_schema = 'EverWellDB_v2.5'
AND table_name = 'Appointment'
AND column_name IN ('google_meet_url', 'google_event_id', 'meeting_id', 'is_virtual')
ORDER BY column_name; 