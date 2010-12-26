using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace GetBackToWork
{
    public static class Utils
    {
        public static void DataTableToCsv(StreamWriter writer, DataTable data)
        {
            DataTableToCsv(writer, data, null);
        }

        public static void DataTableToCsv(StreamWriter writer, DataTable data, IDictionary<string, string> customFormatting)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (data == null)
                throw new ArgumentNullException("data");

            string val;
            foreach (DataRow row in data.Rows)
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    if (customFormatting != null && customFormatting.ContainsKey(data.Columns[i].ColumnName))
                       val = String.Format(customFormatting[data.Columns[i].ColumnName], row[i]);
                    else
                        val = row[i].ToString();
                    writer.Write((i > 0 ? "," : "") + "\"" + val.Replace("\"", "\"\"") + "\"");
                }
                writer.Write(Environment.NewLine);
            }
        }
    }
}
