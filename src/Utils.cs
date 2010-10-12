using System;
using System.Data;
using System.IO;

namespace GetBackToWork
{
    public static class Utils
    {
        public static void DataTableToCsv(StreamWriter writer, DataTable data)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (data == null)
                throw new ArgumentNullException("data");

            foreach (DataRow row in data.Rows)
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                    writer.Write((i > 0 ? "," : "") + "\"" + row.ItemArray[i].ToString().Replace("\"", "\"\"") + "\"");
                writer.Write(Environment.NewLine);
            }
        }
    }
}
