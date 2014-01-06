using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Migrations
{
    public class SqlFactory
    {
        public PimsContext Context { get; private set; }

        public SqlFactory(PimsContext context)
        {
            Context = context;
        }

        public void GenerateAuditTrigger(string tableName, string primaryKeyName1)
        {
            GenerateAuditTrigger(tableName, primaryKeyName1, null);
        }

        public void GenerateAuditTrigger(string tableName, string primaryKeyName1, string primaryKeyName2)
        {
            string table = string.Format("[dbo].[{0}]", tableName);
            string trigger = string.Format("[dbo].[TR_{0}Audit]", tableName);

            StringBuilder builder = new StringBuilder();

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
            builder.AppendLine("    IF NOT (SELECT([value]) FROM [dbo].[Settings] WHERE [key] = 'IsAuditingEnabled') IN ('TRUE', '1')");
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
            builder.AppendLine("    SELECT @inserted = (SELECT * FROM inserted FOR XML RAW)");
            builder.AppendLine("    SELECT @deleted = (SELECT * FROM deleted FOR XML RAW)");
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
    }
}
