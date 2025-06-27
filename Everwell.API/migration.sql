CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'EverWellDB_v1') THEN
        CREATE SCHEMA "EverWellDB_v1";
    END IF;
END $EF$;

CREATE TABLE "EverWellDB_v1"."Service" (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    description character varying(256) NOT NULL,
    price numeric NOT NULL,
    is_active boolean NOT NULL,
    created_at date NOT NULL,
    updated_at date NOT NULL,
    CONSTRAINT "PK_Service" PRIMARY KEY (id)
);

CREATE TABLE "EverWellDB_v1"."Users" (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    email character varying(256) NOT NULL,
    phone_number character varying(10) NOT NULL,
    address text NOT NULL,
    password text NOT NULL,
    role integer NOT NULL,
    avatar_url character varying(1000),
    is_active boolean NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY (id)
);

CREATE TABLE "EverWellDB_v1".appointment (
    appointment_id uuid NOT NULL,
    customer_id uuid NOT NULL,
    consultant_id uuid NOT NULL,
    service_id uuid NOT NULL,
    appointment_date date NOT NULL,
    start_time time without time zone NOT NULL,
    end_time time without time zone NOT NULL,
    status integer NOT NULL,
    notes text,
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_appointment" PRIMARY KEY (appointment_id),
    CONSTRAINT "FK_appointment_Service_service_id" FOREIGN KEY (service_id) REFERENCES "EverWellDB_v1"."Service" (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_appointment_Users_consultant_id" FOREIGN KEY (consultant_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_appointment_Users_customer_id" FOREIGN KEY (customer_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE RESTRICT
);

CREATE TABLE "EverWellDB_v1".menstrual_cycle_tracking (
    tracking_id uuid NOT NULL,
    customer_id uuid NOT NULL,
    cycle_start_date date NOT NULL,
    cycle_end_date date,
    symptoms text,
    notes text,
    created_at timestamp with time zone NOT NULL,
    notify_before_days integer,
    notification_enabled boolean NOT NULL,
    CONSTRAINT "PK_menstrual_cycle_tracking" PRIMARY KEY (tracking_id),
    CONSTRAINT "FK_menstrual_cycle_tracking_Users_customer_id" FOREIGN KEY (customer_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE RESTRICT
);

CREATE TABLE "EverWellDB_v1"."Post" (
    id uuid NOT NULL,
    title text NOT NULL,
    content text NOT NULL,
    "PostStatus" integer NOT NULL,
    "PostCategory" integer NOT NULL,
    staff_id uuid NOT NULL,
    created_at date NOT NULL,
    CONSTRAINT "PK_Post" PRIMARY KEY (id),
    CONSTRAINT "FK_Post_Users_staff_id" FOREIGN KEY (staff_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE RESTRICT
);

CREATE TABLE "EverWellDB_v1".questions (
    question_id uuid NOT NULL,
    customer_id uuid NOT NULL,
    consultant_id uuid NOT NULL,
    title character varying(255) NOT NULL,
    question_text text NOT NULL,
    answer_text text,
    status integer NOT NULL,
    created_at timestamp with time zone NOT NULL,
    answered_at timestamp with time zone,
    CONSTRAINT "PK_questions" PRIMARY KEY (question_id),
    CONSTRAINT "FK_questions_Users_consultant_id" FOREIGN KEY (consultant_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE SET NULL,
    CONSTRAINT "FK_questions_Users_customer_id" FOREIGN KEY (customer_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE RESTRICT
);

CREATE TABLE "EverWellDB_v1"."Feedback" (
    id uuid NOT NULL,
    customer_id uuid NOT NULL,
    consultant_id uuid NOT NULL,
    service_id uuid NOT NULL,
    appoinement_id uuid NOT NULL,
    "AppointmentId1" uuid NOT NULL,
    rating integer NOT NULL,
    comment text NOT NULL,
    created_at date NOT NULL,
    CONSTRAINT "PK_Feedback" PRIMARY KEY (id),
    CONSTRAINT "FK_Feedback_Service_service_id" FOREIGN KEY (service_id) REFERENCES "EverWellDB_v1"."Service" (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_Feedback_Users_consultant_id" FOREIGN KEY (consultant_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_Feedback_Users_customer_id" FOREIGN KEY (customer_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_Feedback_appointment_AppointmentId1" FOREIGN KEY ("AppointmentId1") REFERENCES "EverWellDB_v1".appointment (appointment_id) ON DELETE CASCADE,
    CONSTRAINT "FK_Feedback_appointment_appoinement_id" FOREIGN KEY (appoinement_id) REFERENCES "EverWellDB_v1".appointment (appointment_id) ON DELETE RESTRICT
);

CREATE TABLE "EverWellDB_v1"."STITesting" (
    id uuid NOT NULL,
    appointment_id uuid NOT NULL,
    customer_id uuid NOT NULL,
    test_type integer NOT NULL,
    method integer NOT NULL,
    status integer NOT NULL,
    collected_date date,
    CONSTRAINT "PK_STITesting" PRIMARY KEY (id),
    CONSTRAINT "FK_STITesting_Users_customer_id" FOREIGN KEY (customer_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_STITesting_appointment_appointment_id" FOREIGN KEY (appointment_id) REFERENCES "EverWellDB_v1".appointment (appointment_id) ON DELETE RESTRICT
);

CREATE TABLE "EverWellDB_v1".menstrual_cycle_notification (
    notification_id uuid NOT NULL,
    tracking_id uuid NOT NULL,
    phase integer NOT NULL,
    sent_at timestamp with time zone NOT NULL,
    message text NOT NULL,
    CONSTRAINT "PK_menstrual_cycle_notification" PRIMARY KEY (notification_id),
    CONSTRAINT "FK_menstrual_cycle_notification_menstrual_cycle_tracking_track~" FOREIGN KEY (tracking_id) REFERENCES "EverWellDB_v1".menstrual_cycle_tracking (tracking_id) ON DELETE CASCADE
);

CREATE TABLE "EverWellDB_v1"."TestResults" (
    id uuid NOT NULL,
    sti_testing_id uuid NOT NULL,
    result_data text NOT NULL,
    status integer NOT NULL,
    customer_id uuid,
    staff_id uuid,
    examined_at timestamp with time zone,
    sent_at timestamp with time zone,
    "UserId" uuid,
    "UserId1" uuid,
    CONSTRAINT "PK_TestResults" PRIMARY KEY (id),
    CONSTRAINT "FK_TestResults_STITesting_sti_testing_id" FOREIGN KEY (sti_testing_id) REFERENCES "EverWellDB_v1"."STITesting" (id) ON DELETE CASCADE,
    CONSTRAINT "FK_TestResults_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "EverWellDB_v1"."Users" (id),
    CONSTRAINT "FK_TestResults_Users_UserId1" FOREIGN KEY ("UserId1") REFERENCES "EverWellDB_v1"."Users" (id),
    CONSTRAINT "FK_TestResults_Users_customer_id" FOREIGN KEY (customer_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE SET NULL,
    CONSTRAINT "FK_TestResults_Users_staff_id" FOREIGN KEY (staff_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE SET NULL
);

CREATE INDEX "IX_appointment_consultant_id" ON "EverWellDB_v1".appointment (consultant_id);

CREATE INDEX "IX_appointment_customer_id" ON "EverWellDB_v1".appointment (customer_id);

CREATE INDEX "IX_appointment_service_id" ON "EverWellDB_v1".appointment (service_id);

CREATE INDEX "IX_Feedback_appoinement_id" ON "EverWellDB_v1"."Feedback" (appoinement_id);

CREATE INDEX "IX_Feedback_AppointmentId1" ON "EverWellDB_v1"."Feedback" ("AppointmentId1");

CREATE INDEX "IX_Feedback_consultant_id" ON "EverWellDB_v1"."Feedback" (consultant_id);

CREATE INDEX "IX_Feedback_customer_id" ON "EverWellDB_v1"."Feedback" (customer_id);

CREATE INDEX "IX_Feedback_service_id" ON "EverWellDB_v1"."Feedback" (service_id);

CREATE INDEX "IX_menstrual_cycle_notification_tracking_id" ON "EverWellDB_v1".menstrual_cycle_notification (tracking_id);

CREATE INDEX "IX_menstrual_cycle_tracking_customer_id" ON "EverWellDB_v1".menstrual_cycle_tracking (customer_id);

CREATE INDEX "IX_Post_staff_id" ON "EverWellDB_v1"."Post" (staff_id);

CREATE INDEX "IX_questions_consultant_id" ON "EverWellDB_v1".questions (consultant_id);

CREATE INDEX "IX_questions_customer_id" ON "EverWellDB_v1".questions (customer_id);

CREATE INDEX "IX_STITesting_appointment_id" ON "EverWellDB_v1"."STITesting" (appointment_id);

CREATE INDEX "IX_STITesting_customer_id" ON "EverWellDB_v1"."STITesting" (customer_id);

CREATE INDEX "IX_TestResults_customer_id" ON "EverWellDB_v1"."TestResults" (customer_id);

CREATE INDEX "IX_TestResults_staff_id" ON "EverWellDB_v1"."TestResults" (staff_id);

CREATE INDEX "IX_TestResults_sti_testing_id" ON "EverWellDB_v1"."TestResults" (sti_testing_id);

CREATE INDEX "IX_TestResults_UserId" ON "EverWellDB_v1"."TestResults" ("UserId");

CREATE INDEX "IX_TestResults_UserId1" ON "EverWellDB_v1"."TestResults" ("UserId1");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250602165937_newEntities', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE "EverWellDB_v1"."Feedback" DROP CONSTRAINT "FK_Feedback_appointment_appoinement_id";

ALTER TABLE "EverWellDB_v1"."Feedback" RENAME COLUMN appoinement_id TO appointment_id;

ALTER INDEX "EverWellDB_v1"."IX_Feedback_appoinement_id" RENAME TO "IX_Feedback_appointment_id";

ALTER TABLE "EverWellDB_v1"."Feedback" ADD CONSTRAINT "FK_Feedback_appointment_appointment_id" FOREIGN KEY (appointment_id) REFERENCES "EverWellDB_v1".appointment (appointment_id) ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250602171810_fixedFeedback', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE "EverWellDB_v1"."Feedback" DROP CONSTRAINT "FK_Feedback_appointment_AppointmentId1";

ALTER TABLE "EverWellDB_v1"."Feedback" DROP CONSTRAINT "FK_Feedback_appointment_appointment_id";

DROP INDEX "EverWellDB_v1"."IX_Feedback_AppointmentId1";

ALTER TABLE "EverWellDB_v1"."Feedback" DROP COLUMN "AppointmentId1";

ALTER TABLE "EverWellDB_v1"."Feedback" ADD CONSTRAINT "FK_Feedback_appointment_appointment_id" FOREIGN KEY (appointment_id) REFERENCES "EverWellDB_v1".appointment (appointment_id) ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250602172527_fixedFeedbackv2', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE "EverWellDB_v1".appointment DROP CONSTRAINT "FK_appointment_Service_service_id";

ALTER TABLE "EverWellDB_v1".appointment DROP CONSTRAINT "FK_appointment_Users_consultant_id";

ALTER TABLE "EverWellDB_v1".appointment DROP CONSTRAINT "FK_appointment_Users_customer_id";

ALTER TABLE "EverWellDB_v1"."Feedback" DROP CONSTRAINT "FK_Feedback_appointment_appointment_id";

ALTER TABLE "EverWellDB_v1".menstrual_cycle_notification DROP CONSTRAINT "FK_menstrual_cycle_notification_menstrual_cycle_tracking_track~";

ALTER TABLE "EverWellDB_v1".menstrual_cycle_tracking DROP CONSTRAINT "FK_menstrual_cycle_tracking_Users_customer_id";

ALTER TABLE "EverWellDB_v1".questions DROP CONSTRAINT "FK_questions_Users_consultant_id";

ALTER TABLE "EverWellDB_v1".questions DROP CONSTRAINT "FK_questions_Users_customer_id";

ALTER TABLE "EverWellDB_v1"."STITesting" DROP CONSTRAINT "FK_STITesting_appointment_appointment_id";

ALTER TABLE "EverWellDB_v1".questions DROP CONSTRAINT "PK_questions";

ALTER TABLE "EverWellDB_v1".appointment DROP CONSTRAINT "PK_appointment";

ALTER TABLE "EverWellDB_v1".menstrual_cycle_tracking DROP CONSTRAINT "PK_menstrual_cycle_tracking";

ALTER TABLE "EverWellDB_v1".menstrual_cycle_notification DROP CONSTRAINT "PK_menstrual_cycle_notification";

ALTER TABLE "EverWellDB_v1".questions RENAME TO "Questions";

ALTER TABLE "EverWellDB_v1".appointment RENAME TO "Appointment";

ALTER TABLE "EverWellDB_v1".menstrual_cycle_tracking RENAME TO "MenstrualCycleTracking";

ALTER TABLE "EverWellDB_v1".menstrual_cycle_notification RENAME TO "MenstrualCycleNotification";

ALTER INDEX "EverWellDB_v1"."IX_questions_customer_id" RENAME TO "IX_Questions_customer_id";

ALTER INDEX "EverWellDB_v1"."IX_questions_consultant_id" RENAME TO "IX_Questions_consultant_id";

ALTER INDEX "EverWellDB_v1"."IX_appointment_service_id" RENAME TO "IX_Appointment_service_id";

ALTER INDEX "EverWellDB_v1"."IX_appointment_customer_id" RENAME TO "IX_Appointment_customer_id";

ALTER INDEX "EverWellDB_v1"."IX_appointment_consultant_id" RENAME TO "IX_Appointment_consultant_id";

ALTER INDEX "EverWellDB_v1"."IX_menstrual_cycle_tracking_customer_id" RENAME TO "IX_MenstrualCycleTracking_customer_id";

ALTER INDEX "EverWellDB_v1"."IX_menstrual_cycle_notification_tracking_id" RENAME TO "IX_MenstrualCycleNotification_tracking_id";

ALTER TABLE "EverWellDB_v1"."Questions" ADD CONSTRAINT "PK_Questions" PRIMARY KEY (question_id);

ALTER TABLE "EverWellDB_v1"."Appointment" ADD CONSTRAINT "PK_Appointment" PRIMARY KEY (appointment_id);

ALTER TABLE "EverWellDB_v1"."MenstrualCycleTracking" ADD CONSTRAINT "PK_MenstrualCycleTracking" PRIMARY KEY (tracking_id);

ALTER TABLE "EverWellDB_v1"."MenstrualCycleNotification" ADD CONSTRAINT "PK_MenstrualCycleNotification" PRIMARY KEY (notification_id);

ALTER TABLE "EverWellDB_v1"."Appointment" ADD CONSTRAINT "FK_Appointment_Service_service_id" FOREIGN KEY (service_id) REFERENCES "EverWellDB_v1"."Service" (id) ON DELETE RESTRICT;

ALTER TABLE "EverWellDB_v1"."Appointment" ADD CONSTRAINT "FK_Appointment_Users_consultant_id" FOREIGN KEY (consultant_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE RESTRICT;

ALTER TABLE "EverWellDB_v1"."Appointment" ADD CONSTRAINT "FK_Appointment_Users_customer_id" FOREIGN KEY (customer_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE RESTRICT;

ALTER TABLE "EverWellDB_v1"."Feedback" ADD CONSTRAINT "FK_Feedback_Appointment_appointment_id" FOREIGN KEY (appointment_id) REFERENCES "EverWellDB_v1"."Appointment" (appointment_id) ON DELETE CASCADE;

ALTER TABLE "EverWellDB_v1"."MenstrualCycleNotification" ADD CONSTRAINT "FK_MenstrualCycleNotification_MenstrualCycleTracking_tracking_~" FOREIGN KEY (tracking_id) REFERENCES "EverWellDB_v1"."MenstrualCycleTracking" (tracking_id) ON DELETE CASCADE;

ALTER TABLE "EverWellDB_v1"."MenstrualCycleTracking" ADD CONSTRAINT "FK_MenstrualCycleTracking_Users_customer_id" FOREIGN KEY (customer_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE RESTRICT;

ALTER TABLE "EverWellDB_v1"."Questions" ADD CONSTRAINT "FK_Questions_Users_consultant_id" FOREIGN KEY (consultant_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE SET NULL;

ALTER TABLE "EverWellDB_v1"."Questions" ADD CONSTRAINT "FK_Questions_Users_customer_id" FOREIGN KEY (customer_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE RESTRICT;

ALTER TABLE "EverWellDB_v1"."STITesting" ADD CONSTRAINT "FK_STITesting_Appointment_appointment_id" FOREIGN KEY (appointment_id) REFERENCES "EverWellDB_v1"."Appointment" (appointment_id) ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250602172914_NamesV2', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE "EverWellDB_v1"."Appointment" DROP COLUMN end_time;

ALTER TABLE "EverWellDB_v1"."Appointment" DROP COLUMN start_time;

ALTER TABLE "EverWellDB_v1"."Appointment" ADD shift_slot integer NOT NULL DEFAULT 0;

CREATE TABLE "EverWellDB_v1"."ConsultantSchedule" (
    schedule_id uuid NOT NULL,
    consultant_id uuid NOT NULL,
    work_date date NOT NULL,
    shift_slot integer NOT NULL,
    is_available boolean NOT NULL,
    CONSTRAINT "PK_ConsultantSchedule" PRIMARY KEY (schedule_id),
    CONSTRAINT "FK_ConsultantSchedule_Users_consultant_id" FOREIGN KEY (consultant_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_ConsultantSchedule_consultant_id_work_date_shift_slot" ON "EverWellDB_v1"."ConsultantSchedule" (consultant_id, work_date, shift_slot);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250605115858_updatedappointment', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE "EverWellDB_v1"."Appointment" RENAME COLUMN appointment_id TO id;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250605142524_updatedappointmentv2', '8.0.11');

COMMIT;

START TRANSACTION;

CREATE TABLE "EverWellDB_v1"."Notifications" (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    title character varying(100) NOT NULL,
    message character varying(500) NOT NULL,
    notification_type integer NOT NULL,
    priority integer NOT NULL,
    created_at timestamp with time zone NOT NULL,
    is_read boolean NOT NULL,
    appointment_id uuid,
    test_result_id uuid,
    CONSTRAINT "PK_Notifications" PRIMARY KEY (id),
    CONSTRAINT "FK_Notifications_Appointment_appointment_id" FOREIGN KEY (appointment_id) REFERENCES "EverWellDB_v1"."Appointment" (id) ON DELETE SET NULL,
    CONSTRAINT "FK_Notifications_TestResults_test_result_id" FOREIGN KEY (test_result_id) REFERENCES "EverWellDB_v1"."TestResults" (id) ON DELETE SET NULL,
    CONSTRAINT "FK_Notifications_Users_user_id" FOREIGN KEY (user_id) REFERENCES "EverWellDB_v1"."Users" (id) ON DELETE CASCADE
);

CREATE INDEX "IX_Notifications_appointment_id" ON "EverWellDB_v1"."Notifications" (appointment_id);

CREATE INDEX "IX_Notifications_created_at" ON "EverWellDB_v1"."Notifications" (created_at);

CREATE INDEX "IX_Notifications_is_read" ON "EverWellDB_v1"."Notifications" (is_read);

CREATE INDEX "IX_Notifications_test_result_id" ON "EverWellDB_v1"."Notifications" (test_result_id);

CREATE INDEX "IX_Notifications_user_id" ON "EverWellDB_v1"."Notifications" (user_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250613091649_notifications', '8.0.11');

COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'EverWellDB_v2') THEN
        CREATE SCHEMA "EverWellDB_v2";
    END IF;
END $EF$;

ALTER TABLE "EverWellDB_v1"."Users" SET SCHEMA "EverWellDB_v2";

ALTER TABLE "EverWellDB_v1"."TestResults" SET SCHEMA "EverWellDB_v2";

ALTER TABLE "EverWellDB_v1"."STITesting" SET SCHEMA "EverWellDB_v2";

ALTER TABLE "EverWellDB_v1"."Service" SET SCHEMA "EverWellDB_v2";

ALTER TABLE "EverWellDB_v1"."Questions" SET SCHEMA "EverWellDB_v2";

ALTER TABLE "EverWellDB_v1"."Post" SET SCHEMA "EverWellDB_v2";

ALTER TABLE "EverWellDB_v1"."Notifications" SET SCHEMA "EverWellDB_v2";

ALTER TABLE "EverWellDB_v1"."MenstrualCycleTracking" SET SCHEMA "EverWellDB_v2";

ALTER TABLE "EverWellDB_v1"."MenstrualCycleNotification" SET SCHEMA "EverWellDB_v2";

ALTER TABLE "EverWellDB_v1"."Feedback" SET SCHEMA "EverWellDB_v2";

ALTER TABLE "EverWellDB_v1"."ConsultantSchedule" SET SCHEMA "EverWellDB_v2";

ALTER TABLE "EverWellDB_v1"."BlacklistedTokens" SET SCHEMA "EverWellDB_v2";

ALTER TABLE "EverWellDB_v1"."Appointment" SET SCHEMA "EverWellDB_v2";

ALTER TABLE "EverWellDB_v2"."Users" RENAME COLUMN role TO role_id;

CREATE TABLE "EverWellDB_v2"."Roles" (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    name integer NOT NULL,
    CONSTRAINT "PK_Roles" PRIMARY KEY (id)
);

INSERT INTO "EverWellDB_v2"."Roles" (id, name)
VALUES (1, 0);
INSERT INTO "EverWellDB_v2"."Roles" (id, name)
VALUES (2, 1);
INSERT INTO "EverWellDB_v2"."Roles" (id, name)
VALUES (3, 2);
INSERT INTO "EverWellDB_v2"."Roles" (id, name)
VALUES (4, 3);
INSERT INTO "EverWellDB_v2"."Roles" (id, name)
VALUES (5, 4);


                UPDATE "EverWellDB_v2"."Users"
                SET role_id = 
                    CASE 
                        WHEN role_id = 0 THEN 1  -- Customer
                        WHEN role_id = 1 THEN 2  -- Consultant
                        WHEN role_id = 2 THEN 3  -- Staff
                        WHEN role_id = 3 THEN 4  -- Manager
                        WHEN role_id = 4 THEN 5  -- Admin
                        ELSE 1                   -- Default to Customer if invalid
                    END;
            

CREATE INDEX "IX_Users_role_id" ON "EverWellDB_v2"."Users" (role_id);

ALTER TABLE "EverWellDB_v2"."Users" ADD CONSTRAINT "FK_Users_Roles_role_id" FOREIGN KEY (role_id) REFERENCES "EverWellDB_v2"."Roles" (id) ON DELETE RESTRICT;

SELECT setval(
    pg_get_serial_sequence('"EverWellDB_v2"."Roles"', 'id'),
    GREATEST(
        (SELECT MAX(id) FROM "EverWellDB_v2"."Roles") + 1,
        nextval(pg_get_serial_sequence('"EverWellDB_v2"."Roles"', 'id'))),
    false);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250616122951_RolesMigrations', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE "EverWellDB_v2"."Appointment" DROP CONSTRAINT "FK_Appointment_Service_service_id";

ALTER TABLE "EverWellDB_v2"."Feedback" DROP CONSTRAINT "FK_Feedback_Service_service_id";

DROP TABLE "EverWellDB_v2"."Service";

DROP INDEX "EverWellDB_v2"."IX_Feedback_service_id";

DROP INDEX "EverWellDB_v2"."IX_Appointment_service_id";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250616160359_RemoveService', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE "EverWellDB_v2"."Feedback" DROP COLUMN service_id;

ALTER TABLE "EverWellDB_v2"."Appointment" DROP COLUMN service_id;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250616160824_RemoveServiceV2', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE "EverWellDB_v2"."STITesting" DROP CONSTRAINT "FK_STITesting_Users_customer_id";

ALTER TABLE "EverWellDB_v2"."TestResults" DROP CONSTRAINT "FK_TestResults_Users_customer_id";

DROP INDEX "EverWellDB_v2"."IX_TestResults_customer_id";

DROP INDEX "EverWellDB_v2"."IX_STITesting_customer_id";

ALTER TABLE "EverWellDB_v2"."TestResults" DROP COLUMN customer_id;

ALTER TABLE "EverWellDB_v2"."STITesting" DROP COLUMN customer_id;

ALTER TABLE "EverWellDB_v2"."STITesting" ADD "UserId" uuid;

CREATE INDEX "IX_STITesting_UserId" ON "EverWellDB_v2"."STITesting" ("UserId");

ALTER TABLE "EverWellDB_v2"."STITesting" ADD CONSTRAINT "FK_STITesting_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "EverWellDB_v2"."Users" (id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250616172746_UpdateSTITesting', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE "EverWellDB_v2"."STITesting" DROP CONSTRAINT "FK_STITesting_Appointment_appointment_id";

ALTER TABLE "EverWellDB_v2"."TestResults" DROP COLUMN examined_at;

ALTER TABLE "EverWellDB_v2"."TestResults" DROP COLUMN result_data;

ALTER TABLE "EverWellDB_v2"."TestResults" RENAME COLUMN status TO outcome;

ALTER TABLE "EverWellDB_v2"."TestResults" RENAME COLUMN sent_at TO processed_at;

ALTER TABLE "EverWellDB_v2"."STITesting" RENAME COLUMN test_type TO test_package;

ALTER TABLE "EverWellDB_v2"."STITesting" RENAME COLUMN method TO slot;

ALTER TABLE "EverWellDB_v2"."STITesting" RENAME COLUMN appointment_id TO customer_id;

ALTER INDEX "EverWellDB_v2"."IX_STITesting_appointment_id" RENAME TO "IX_STITesting_customer_id";

ALTER TABLE "EverWellDB_v2"."TestResults" ADD comments character varying(500);

ALTER TABLE "EverWellDB_v2"."TestResults" ADD parameter integer[] NOT NULL DEFAULT ARRAY[]::integer[];

UPDATE "EverWellDB_v2"."STITesting" SET collected_date = DATE '-infinity' WHERE collected_date IS NULL;
ALTER TABLE "EverWellDB_v2"."STITesting" ALTER COLUMN collected_date SET NOT NULL;
ALTER TABLE "EverWellDB_v2"."STITesting" ALTER COLUMN collected_date SET DEFAULT DATE '-infinity';

ALTER TABLE "EverWellDB_v2"."STITesting" ADD completed_at timestamp with time zone;

ALTER TABLE "EverWellDB_v2"."STITesting" ADD created_at timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';

ALTER TABLE "EverWellDB_v2"."STITesting" ADD is_completed boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "EverWellDB_v2"."STITesting" ADD notes character varying(500);

ALTER TABLE "EverWellDB_v2"."STITesting" ADD sample_taken_at timestamp with time zone;

ALTER TABLE "EverWellDB_v2"."STITesting" ADD CONSTRAINT "FK_STITesting_Users_customer_id" FOREIGN KEY (customer_id) REFERENCES "EverWellDB_v2"."Users" (id) ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250617105958_updateSTI_TestResult', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE "EverWellDB_v2"."Notifications" ADD stitesting_id uuid;

CREATE INDEX "IX_Notifications_stitesting_id" ON "EverWellDB_v2"."Notifications" (stitesting_id);

ALTER TABLE "EverWellDB_v2"."Notifications" ADD CONSTRAINT "FK_Notifications_STITesting_stitesting_id" FOREIGN KEY (stitesting_id) REFERENCES "EverWellDB_v2"."STITesting" (id) ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250617114955_updateNotifications', '8.0.11');

COMMIT;

START TRANSACTION;

CREATE TABLE "EverWellDB_v2"."STITesting" (
    id uuid NOT NULL,
    customer_id uuid NOT NULL,
    schedule_date date NOT NULL,
    slot integer NOT NULL,
    test_package text NOT NULL,
    status text NOT NULL,
    notes character varying(500),
    is_paid boolean NOT NULL DEFAULT FALSE,
    total_price numeric(18,2) NOT NULL DEFAULT 0.0,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    CONSTRAINT "PK_STITesting" PRIMARY KEY (id),
    CONSTRAINT "FK_STITesting_Users_customer_id" FOREIGN KEY (customer_id) REFERENCES "EverWellDB_v2"."Users" (id) ON DELETE CASCADE
);

CREATE TABLE "EverWellDB_v2"."TestResults" (
    id uuid NOT NULL,
    sti_testing_id uuid NOT NULL,
    parameter integer NOT NULL,
    outcome text NOT NULL,
    comments character varying(500),
    staff_id uuid,
    processed_at timestamp with time zone,
    CONSTRAINT "PK_TestResults" PRIMARY KEY (id),
    CONSTRAINT "FK_TestResults_STITesting_sti_testing_id" FOREIGN KEY (sti_testing_id) REFERENCES "EverWellDB_v2"."STITesting" (id) ON DELETE CASCADE,
    CONSTRAINT "FK_TestResults_Users_staff_id" FOREIGN KEY (staff_id) REFERENCES "EverWellDB_v2"."Users" (id) ON DELETE SET NULL
);

CREATE INDEX "IX_STITesting_customer_id" ON "EverWellDB_v2"."STITesting" (customer_id);

CREATE INDEX "IX_TestResults_sti_testing_id" ON "EverWellDB_v2"."TestResults" (sti_testing_id);

CREATE INDEX "IX_TestResults_staff_id" ON "EverWellDB_v2"."TestResults" (staff_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250618141307_AddSTIandTestResult', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE "EverWellDB_v2"."STITesting" RENAME COLUMN collected_date TO schedule_date;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250618151514_update', '8.0.11');

COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'EverWellDB_v2.5') THEN
        CREATE SCHEMA "EverWellDB_v2.5";
    END IF;
END $EF$;

ALTER TABLE "EverWellDB_v2"."Users" SET SCHEMA "EverWellDB_v2.5";

ALTER TABLE "EverWellDB_v2"."TestResults" SET SCHEMA "EverWellDB_v2.5";

ALTER TABLE "EverWellDB_v2"."STITesting" SET SCHEMA "EverWellDB_v2.5";

ALTER TABLE "EverWellDB_v2"."Roles" SET SCHEMA "EverWellDB_v2.5";

ALTER TABLE "EverWellDB_v2"."Questions" SET SCHEMA "EverWellDB_v2.5";

ALTER TABLE "EverWellDB_v2"."Post" SET SCHEMA "EverWellDB_v2.5";

ALTER TABLE "EverWellDB_v2"."Notifications" SET SCHEMA "EverWellDB_v2.5";

ALTER TABLE "EverWellDB_v2"."MenstrualCycleTracking" SET SCHEMA "EverWellDB_v2.5";

ALTER TABLE "EverWellDB_v2"."MenstrualCycleNotification" SET SCHEMA "EverWellDB_v2.5";

ALTER TABLE "EverWellDB_v2"."Feedback" SET SCHEMA "EverWellDB_v2.5";

ALTER TABLE "EverWellDB_v2"."ConsultantSchedule" SET SCHEMA "EverWellDB_v2.5";

ALTER TABLE "EverWellDB_v2"."BlacklistedTokens" SET SCHEMA "EverWellDB_v2.5";

ALTER TABLE "EverWellDB_v2"."Appointment" SET SCHEMA "EverWellDB_v2.5";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250618174141_test', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE "EverWellDB_v2.5"."Post" ALTER COLUMN created_at TYPE timestamp with time zone;

ALTER TABLE "EverWellDB_v2.5"."Post" ADD image_url text NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250619174306_updatePostEntity', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE "EverWellDB_v2.5"."TestResults" ALTER COLUMN parameter TYPE text;

ALTER TABLE "EverWellDB_v2.5"."Roles" ALTER COLUMN name TYPE text;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250621142648_updateEnumProperties', '8.0.11');

COMMIT;

START TRANSACTION;

ALTER TABLE "EverWellDB_v2.5"."Questions" ALTER COLUMN consultant_id DROP NOT NULL;

ALTER TABLE "EverWellDB_v2.5"."Notifications" ADD question_id uuid;

CREATE INDEX "IX_Notifications_question_id" ON "EverWellDB_v2.5"."Notifications" (question_id);

ALTER TABLE "EverWellDB_v2.5"."Notifications" ADD CONSTRAINT "FK_Notifications_Questions_question_id" FOREIGN KEY (question_id) REFERENCES "EverWellDB_v2.5"."Questions" (question_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250622082200_MakeConsultantIdNullableInQuestions', '8.0.11');

COMMIT;

START TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250624090750_NullableCommentFeedback', '8.0.11');

COMMIT;

