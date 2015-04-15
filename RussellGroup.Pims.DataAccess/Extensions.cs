using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess
{
    public static class Extensions
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> source)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();

            DataTable output = new DataTable();

            foreach (var property in properties)
            {
                output.Columns.Add(property.Name, property.PropertyType);
            }

            foreach (var item in source)
            {
                DataRow row = output.NewRow();

                foreach (var property in properties)
                {
                    row[property.Name] = property.GetValue(item, null);
                }

                output.Rows.Add(row);
            }

            return output;
        }

        public static byte[] ToCsv<T>(this IEnumerable<T> source, bool hasHeader = true)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();

            using (var memory = new MemoryStream())
            {
                using (var writer = new StreamWriter(memory))
                {
                    using (var csv = new CsvWriter(writer))
                    {
                        if (hasHeader)
                        {
                            // write the header
                            foreach (var property in properties)
                            {
                                csv.WriteField(property.Name);
                            }

                            csv.NextRecord();
                        }

                        // then the rows
                        foreach (var item in source)
                        {
                            foreach (var property in properties)
                            {
                                var value = property.GetValue(item, null);

                                if (value is DateTime) value = ((DateTime)value).ToShortDateString();

                                csv.WriteField(value);
                            }

                            csv.NextRecord();
                        }
                    }
                }

                return memory.ToArray();
            }
        }
    }
}
