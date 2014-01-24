using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataMigration
{
    public static class Extensions
    {
        public static string GetValue(this OleDbDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);

            if (!reader.IsDBNull(ordinal))
            {
                return reader.GetValue(ordinal).ToString().Trim();
            }

            return null;
        }

        public static DateTime? GetDateTime(this OleDbDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);

            if (!reader.IsDBNull(ordinal))
            {
                var value = reader.GetValue(ordinal).ToString();
                var result = DateTime.Parse(value);

                if (result < System.Data.SqlTypes.SqlDateTime.MinValue ||
                    result > System.Data.SqlTypes.SqlDateTime.MaxValue)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return result;
            }

            return null;
        }

        public static Nullable<T> GetValueOrNull<T>(this OleDbDataReader reader, string name) where T : struct
        {
            var ordinal = reader.GetOrdinal(name);

            if (!reader.IsDBNull(ordinal))
            {
                var value = reader.GetValue(ordinal).ToString();
                var result = (T)Convert.ChangeType(value, typeof(T));

                return result;
            }

            return null;
        }
    }
}
