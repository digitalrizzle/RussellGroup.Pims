using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Migrations
{
    public class SqlFactory
    {
        public PimsDbContext Context { get; private set; }
        public string AuditTableName { get; private set; }
        public string SettingsTableName { get; private set; }

        public SqlFactory(PimsDbContext context, string auditTableName, string settingsTableName)
        {
            Context = context;
            AuditTableName = auditTableName;
            SettingsTableName = settingsTableName;
        }

        public void GenerateAuditTrigger<T>() where T : class
        {
            var keyNames = GetKeyNames<T>();

            var tableName = typeof(T).Name;
            var primaryKeyName1 = keyNames[0];
            var primaryKeyName2 = keyNames.Length > 1 ? keyNames[1] : null;

            GenerateAuditTrigger(tableName, primaryKeyName1, primaryKeyName2);
        }

        public void GenerateAuditTrigger(string tableName, string primaryKeyName1)
        {
            GenerateAuditTrigger(tableName, primaryKeyName1, null);
        }

        public void GenerateAuditTrigger(string tableName, string primaryKeyName1, string primaryKeyName2 = null)
        {
            var table = string.Format("[dbo].[{0}]", tableName);
            var trigger = string.Format("[dbo].[TR_{0}{1}]", tableName, AuditTableName);

            var builder = new StringBuilder();

            builder.AppendFormatLine("IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'{0}'))", trigger);
            builder.AppendFormatLine("DROP TRIGGER {0}", trigger);

            Context.Database.ExecuteSqlCommand(builder.ToString());
            builder.Clear();

            builder.AppendFormatLine("CREATE TRIGGER {0}", trigger);
            builder.AppendFormatLine("    ON {0}", table);
            builder.AppendLine("    FOR INSERT, DELETE, UPDATE");
            builder.AppendLine("AS");
            builder.AppendLine("    BEGIN");
            builder.AppendLine();
            builder.AppendLine("    SET NOCOUNT ON;");
            builder.AppendLine();
            builder.AppendLine("    IF NOT (SELECT([value]) FROM [dbo].[Settings] WHERE [Key] = 'IsAuditingEnabled') IN ('TRUE', '1')");
            builder.AppendLine("        RETURN");
            builder.AppendLine();
            builder.AppendLine("    DECLARE");
            builder.AppendLine("        @action CHAR(1),");
            builder.AppendLine("        @inserted XML,");
            builder.AppendLine("        @deleted XML");
            builder.AppendLine();
            builder.AppendLine("    DECLARE @context VARBINARY(128), @userName NVARCHAR(50)");
            builder.AppendLine("    SELECT @context = CONTEXT_INFO FROM master.dbo.sysprocesses WHERE spid = @@spid");
            builder.AppendLine("    SET @userName = CAST(@context as NVARCHAR(50))");
            builder.AppendLine();
            builder.AppendLine("    SELECT @inserted = (SELECT * FROM inserted FOR XML RAW, BINARY BASE64) COLLATE Latin1_General_CS_AS");
            builder.AppendLine("    SELECT @deleted = (SELECT * FROM deleted FOR XML RAW, BINARY BASE64) COLLATE Latin1_General_CS_AS");
            builder.AppendLine();
            builder.AppendLine("    IF CAST(@inserted AS NVARCHAR(MAX)) = CAST(@deleted AS NVARCHAR(MAX))");
            builder.AppendLine("        RETURN");
            builder.AppendLine();
            builder.AppendLine("    IF EXISTS (SELECT * FROM inserted)");
            builder.AppendLine("    BEGIN");
            builder.AppendLine("        IF EXISTS (SELECT * FROM deleted)");
            builder.AppendLine("            SET @action = 'U'");
            builder.AppendLine("        ELSE");
            builder.AppendLine("            SET @action = 'I'");
            builder.AppendLine();
            builder.AppendLine("        INSERT INTO");
            builder.AppendLine("            [dbo].[Audits]");
            builder.AppendLine("        SELECT");
            builder.AppendLine("            getdate(),");
            builder.AppendFormatLine("            '{0}',", tableName);
            builder.AppendFormatLine("            (SELECT TOP 1 [{0}] FROM inserted),", primaryKeyName1);
            builder.AppendFormatLine("            {0},", string.IsNullOrWhiteSpace(primaryKeyName2) ? "NULL" : string.Format("(SELECT TOP 1 [{0}] FROM inserted)", primaryKeyName2));
            builder.AppendLine("            @action,");
            builder.AppendLine("            @deleted,");
            builder.AppendLine("            @inserted,");
            builder.AppendLine("            @userName");
            builder.AppendLine("        FROM");
            builder.AppendLine("            inserted");
            builder.AppendLine("    END");
            builder.AppendLine("    ELSE");
            builder.AppendLine("        SET @action = 'D'");
            builder.AppendLine();
            builder.AppendLine("        INSERT INTO");
            builder.AppendLine("            [dbo].[Audits]");
            builder.AppendLine("        SELECT");
            builder.AppendLine("            getdate(),");
            builder.AppendFormatLine("            '{0}',", tableName);
            builder.AppendFormatLine("            (SELECT TOP 1 [{0}] FROM deleted),", primaryKeyName1);
            builder.AppendFormatLine("            {0},", string.IsNullOrWhiteSpace(primaryKeyName2) ? "NULL" : string.Format("(SELECT TOP 1 [{0}] FROM deleted)", primaryKeyName2));
            builder.AppendLine("            @action,");
            builder.AppendLine("            @deleted,");
            builder.AppendLine("            @inserted,");
            builder.AppendLine("            @userName");
            builder.AppendLine("        FROM");
            builder.AppendLine("            deleted");
            builder.AppendLine("    END");

            Context.Database.ExecuteSqlCommand(builder.ToString());
        }

        public void GenerateSetUserNameContextStoredProcedure()
        {
            var builder = new StringBuilder();

            builder.AppendLine("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetContextUserName]') AND type in (N'P', N'PC'))");
            builder.AppendLine("DROP PROCEDURE [dbo].[SetContextUserName]");

            Context.Database.ExecuteSqlCommand(builder.ToString());
            builder.Clear();

            builder.AppendLine("IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetContextUserName]') AND type in (N'P', N'PC'))");
            builder.AppendLine("BEGIN");
            builder.AppendLine("EXEC dbo.sp_executesql @statement = N'");
            builder.AppendLine("CREATE PROCEDURE [dbo].[SetContextUserName](@userName NVARCHAR(50)) AS");
            builder.AppendLine("BEGIN");
            builder.AppendLine("	DECLARE @m BINARY(128)");
            builder.AppendLine("	SET @m = CAST(@userName AS BINARY(128))");
            builder.AppendLine("	SET CONTEXT_INFO @m");
            builder.AppendLine("END'");
            builder.AppendLine("END");

            Context.Database.ExecuteSqlCommand(builder.ToString());
        }

        private string[] GetKeyNames<T>() where T : class
        {
            Type t = typeof(T);

            var objectContext = ((IObjectContextAdapter)Context).ObjectContext;

            // create method CreateObjectSet with the generic parameter of the base-type
            var method = typeof(ObjectContext).GetMethod("CreateObjectSet", Type.EmptyTypes).MakeGenericMethod(t);
            dynamic objectSet = method.Invoke(objectContext, null);

            IEnumerable<dynamic> keyMembers = objectSet.EntitySet.ElementType.KeyMembers;

            string[] keyNames = keyMembers.Select(k => (string)k.Name).ToArray();

            return keyNames;
        }
    }
}
