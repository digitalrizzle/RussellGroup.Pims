IF EXISTS (SELECT * FROM sys.objects WHERE object_id = object_id(N'[dbo].[SetContextUserName]') AND type IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[SetContextUserName]

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = object_id(N'[dbo].[SetContextUserName]') AND type IN (N'P', N'PC'))
EXEC dbo.sp_executesql @statement =
N'CREATE PROCEDURE [dbo].[SetContextUserName](@userName NVARCHAR(50)) AS
BEGIN
	DECLARE @m BINARY(128)
	SET @m = CAST(@userName AS BINARY(128))
	SET CONTEXT_INFO @m
END'
