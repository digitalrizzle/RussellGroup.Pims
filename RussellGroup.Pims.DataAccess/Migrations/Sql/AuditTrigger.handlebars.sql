IF EXISTS (SELECT * FROM sys.objects WHERE object_id = object_id(N'{{trigger}}') AND type = N'TR')
EXEC dbo.sp_executesql @statement =
N'DROP TRIGGER {{trigger}}'

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = object_id(N'{{trigger}}') AND type = N'TR')
EXEC dbo.sp_executesql @statement =
N'CREATE TRIGGER {{trigger}}
    ON {{table}}
	FOR INSERT, DELETE, UPDATE
AS
BEGIN

SET NOCOUNT ON

IF (SELECT ([value]) FROM [dbo].[Settings] WHERE [Key] = ''IsAuditingEnabled'') IN (''FALSE'', ''0'')
	RETURN

DECLARE
    @action CHAR(1),
    @inserted XML,
    @deleted XML

DECLARE @context VARBINARY(128), @userName NVARCHAR(50)
SELECT @context = CONTEXT_INFO FROM master.dbo.sysprocesses WHERE spid = @@spid
SET @userName = CAST(@context as NVARCHAR(50))

SELECT @inserted = (SELECT * FROM inserted FOR XML RAW, BINARY BASE64) COLLATE Latin1_General_CS_AS
SELECT @deleted = (SELECT * FROM deleted FOR XML RAW, BINARY BASE64) COLLATE Latin1_General_CS_AS

IF CAST(@inserted AS NVARCHAR(MAX)) = CAST(@deleted AS NVARCHAR(MAX))
    RETURN

IF EXISTS (SELECT * FROM inserted)
BEGIN
    IF EXISTS (SELECT * FROM deleted)
        SET @action = ''U''
    ELSE
        SET @action = ''I''

    INSERT INTO
        [dbo].[Audits]
    SELECT
        getdate(),
        ''{{tableName}}'',
        (SELECT TOP 1 [{{primaryKeyName1}}] FROM inserted),
		{{#if primaryKeyName2}}
			(SELECT TOP 1 [{{primaryKeyName2}}] FROM inserted),
		{{else}}
			NULL,
		{{/if}}
        @action,
        @deleted,
        @inserted,
        @userName
    FROM
        inserted

    RETURN
END
ELSE
    SET @action = ''D''

    INSERT INTO
        [dbo].[Audits]
    SELECT
        getdate(),
        ''{{tableName}}'',
        (SELECT TOP 1 [{{primaryKeyName1}}] FROM deleted),
		{{#if primaryKeyName2}}
			(SELECT TOP 1 [{{primaryKeyName2}}] FROM deleted),
		{{else}}
			NULL,
		{{/if}}
        @action,
        @deleted,
        @inserted,
        @userName
    FROM
        deleted
END'
