using RussellGroup.Pims.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RussellGroup.Pims.DataMigration
{
    abstract class AbstractImport
    {
        public PimsDbContext TargetContext { get; protected set; }
        public OleDbConnection SourceConnection { get; set; }
        public string SourceTableName { get; set; }
        public string TargetTableName { get; set; }
        public string SourcePrimaryKeyColumnName { get; set; }

        public void Import(int skip = 0, int take = 0)
        {
            Trace.WriteLine(string.Format("Importing [{0}] => [{1}]...", SourceTableName, TargetTableName));

            int row = 0;
            var sql = string.Format("SELECT * FROM [{0}]{1}", SourceTableName, string.IsNullOrEmpty(SourcePrimaryKeyColumnName) ? string.Empty : string.Format(" ORDER BY [{0}]", SourcePrimaryKeyColumnName));
            var command = new OleDbCommand(sql, SourceConnection);
            var reader = command.ExecuteReader();

            Console.Write("{0}\r", string.Empty.PadLeft(10));

            while (reader.Read())
            {
                if (TargetContext != null) TargetContext.Dispose();

                TargetContext = new PimsDbContext(HttpContext.Current);

                Console.Write("{0}\r", ++row);

                if (row - 1 < skip) continue;

                Map(reader);
                TargetContext.SaveChanges();

                if (take > 0 && row >= skip + take) break;
            }

            TargetContext.SaveChanges();
            TargetContext.Dispose();
        }

        protected abstract void Map(OleDbDataReader reader);

        public AbstractImport Delete()
        {
            var message = string.Format("Deleting from [{0}]...", TargetTableName);
            Trace.WriteLine(message);

            using (var context = new PimsDbContext(HttpContext.Current))
            {
                context.Database.ExecuteSqlCommand(string.Format("DELETE FROM [{0}]", TargetTableName));
                context.Database.ExecuteSqlCommand(string.Format("DBCC CHECKIDENT('{0}', RESEED, 0)", TargetTableName));
            }

            return this;
        }

        public AbstractImport SetAuditing(bool enable)
        {
            var message = string.Format("{0} auditing...", enable ? "Enabling" : "Disabling");
            Trace.WriteLine(message);

            using (var context = new PimsDbContext(HttpContext.Current))
            {
                context.Database.ExecuteSqlCommand(string.Format("UPDATE [Settings] SET [Value] = '{0}' WHERE [Key] = 'IsAuditingEnabled'", enable.ToString()));
            }

            return this;
        }
    }
}
