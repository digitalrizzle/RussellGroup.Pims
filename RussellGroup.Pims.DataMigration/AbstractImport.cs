using RussellGroup.Pims.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataMigration
{
    abstract class AbstractImport
    {
        const int SAVE_CHANGES_OBJECT_LIMIT = 250;

        public PimsContext TargetContext { get; protected set; }
        public OleDbConnection SourceConnection { get; set; }
        public string SourceTableName { get; set; }
        public string TargetTableName { get; set; }
        public string SourcePrimaryKeyColumnName { get; set; }

        public void Import()
        {
            Import(0, 0);
        }
        public void Import(int take)
        {
            Import(0, 0);
        }

        public void Import(int skip, int take)
        {
            Console.WriteLine("[{0}] => [{1}]", SourceTableName, TargetTableName);

            int row = 0;
            var sql = string.Format("SELECT * FROM [{0}]{1}", SourceTableName, string.IsNullOrEmpty(SourcePrimaryKeyColumnName) ? string.Empty : string.Format(" ORDER BY [{0}]", SourcePrimaryKeyColumnName));
            var command = new OleDbCommand(sql, SourceConnection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                if (row % SAVE_CHANGES_OBJECT_LIMIT == 0)
                {
                    if (TargetContext != null) TargetContext.Dispose();
                    TargetContext = new PimsContext();
                }

                Console.Write("{0}\r", ++row);

                if (row - 1 < skip) continue;

                Map(reader);

                if (row % SAVE_CHANGES_OBJECT_LIMIT == 0) SaveChanges();
                if (take > 0 && row >= skip + take) break;
            }

            SaveChanges();
            TargetContext.Dispose();
        }

        private void SaveChanges()
        {
            var message = "Saving...\r";
            Console.Write(message);

            TargetContext.SaveChanges();

            Console.Write("{0}\r", string.Empty.PadRight(message.Length));
        }

        protected abstract void Map(OleDbDataReader reader);

        public AbstractImport Delete()
        {
            var message = string.Format("Deleting from [{0}]...\r", TargetTableName);
            Console.Write(message);

            using (var context = new PimsContext())
            {
                context.Database.ExecuteSqlCommand(string.Format("DELETE FROM [{0}]", TargetTableName));
                context.Database.ExecuteSqlCommand(string.Format("DBCC CHECKIDENT('{0}', RESEED, 0)", TargetTableName));
            }

            Console.Write("{0}\r", string.Empty.PadRight(message.Length));

            return this;
        }
    }
}
