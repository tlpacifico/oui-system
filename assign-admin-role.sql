-- Script to assign Admin role to user thacio.pacifico@gmail.com

-- First, check if user exists, if not create it
DO $$
DECLARE
    v_user_id BIGINT;
    v_admin_role_id BIGINT;
    v_user_external_id UUID := 'eVvFxUokTKfGkzxy6NCIoiPHchl1'::uuid;
    v_user_email TEXT := 'thacio.pacifico@gmail.com';
BEGIN
    -- Check if user exists
    SELECT "Id" INTO v_user_id
    FROM "Users"
    WHERE "Email" = v_user_email;

    -- If user doesn't exist, create it
    IF v_user_id IS NULL THEN
        INSERT INTO "Users" ("ExternalId", "Email", "PasswordHash", "DisplayName", "CreatedOn")
        VALUES (v_user_external_id, v_user_email, '', 'Thacio Pacifico', NOW() AT TIME ZONE 'UTC')
        RETURNING "Id" INTO v_user_id;

        RAISE NOTICE 'Created user: % (ID: %)', v_user_email, v_user_id;
    ELSE
        RAISE NOTICE 'User already exists: % (ID: %)', v_user_email, v_user_id;
    END IF;

    -- Get Admin role ID
    SELECT "Id" INTO v_admin_role_id
    FROM "Roles"
    WHERE "Name" = 'Admin' AND "IsDeleted" = FALSE;

    IF v_admin_role_id IS NULL THEN
        RAISE EXCEPTION 'Admin role not found. Please run the application first to seed roles.';
    END IF;

    RAISE NOTICE 'Admin role ID: %', v_admin_role_id;

    -- Check if user already has Admin role
    IF EXISTS (
        SELECT 1 FROM "UserRoles"
        WHERE "UserId" = v_user_id AND "RoleId" = v_admin_role_id
    ) THEN
        RAISE NOTICE 'User already has Admin role';
    ELSE
        -- Assign Admin role to user
        INSERT INTO "UserRoles" ("UserId", "RoleId", "AssignedOn", "AssignedBy")
        VALUES (v_user_id, v_admin_role_id, NOW() AT TIME ZONE 'UTC', 'system');

        RAISE NOTICE 'Admin role assigned successfully!';
    END IF;

    -- Show user's roles
    RAISE NOTICE 'User roles:';
    PERFORM "Roles"."Name"
    FROM "UserRoles"
    JOIN "Roles" ON "UserRoles"."RoleId" = "Roles"."Id"
    WHERE "UserRoles"."UserId" = v_user_id;

END $$;

-- Verify the assignment
SELECT
    u."Email",
    u."DisplayName",
    r."Name" as "RoleName",
    ur."AssignedOn",
    ur."AssignedBy"
FROM "Users" u
JOIN "UserRoles" ur ON u."Id" = ur."UserId"
JOIN "Roles" r ON ur."RoleId" = r."Id"
WHERE u."Email" = 'thacio.pacifico@gmail.com';
