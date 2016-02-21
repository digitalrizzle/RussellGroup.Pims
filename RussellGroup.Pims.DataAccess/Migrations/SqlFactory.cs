using HandlebarsDotNet;
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
            var keyNames = EntityHelper.Instance.GetKeyNames<T>(Context);

            var tableName = typeof(T).Name;
            var primaryKeyName1 = keyNames[0];
            var primaryKeyName2 = keyNames.Length > 1 ? keyNames[1] : null;

            GenerateAuditTrigger(tableName, primaryKeyName1, primaryKeyName2);
        }

        public void GenerateAuditTrigger(string tableName, string primaryKeyName1 = "Id")
        {
            GenerateAuditTrigger(tableName, primaryKeyName1, null);
        }

        public void GenerateAuditTrigger(string tableName, string primaryKeyName1, string primaryKeyName2 = null)
        {
            var data = new {
                tableName,
                table = string.Format("[dbo].[{0}]", tableName),
                trigger = string.Format("[dbo].[TR_{0}{1}]", tableName, AuditTableName),
                primaryKeyName1 = primaryKeyName1,
                primaryKeyName2 = primaryKeyName2
            };

            var source = Properties.Resources.AuditTrigger_handlebars;
            var template = Handlebars.Compile(source);
            var sql = template(data);

            Context.Database.ExecuteSqlCommand(sql);
        }

        public void GenerateSetUserNameContextStoredProcedure()
        {
            Context.Database.ExecuteSqlCommand(Properties.Resources.SetUserNameContext);
        }
    }
}
