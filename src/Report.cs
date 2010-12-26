using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;

namespace GetBackToWork
{
    class Report
    {
        #region Properties

        public string Filename
        {
            get;
            set;
        }

        public DateTime StartDate
        {
            get;
            set;
        }

        public DateTime EndDate
        {
            get;
            set;
        }

        public decimal Fluff
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public Report(string filename, DateTime start, DateTime end) : this(filename, start, end, 0)
        {
        }

        public Report(string filename, DateTime start, DateTime end, decimal fluff)
        {
            Filename = filename;
            StartDate = start;
            EndDate = end;
            Fluff = fluff;
        }

        #endregion

        #region Methods

        public void Create()
        {
            DateTime dateIndex = StartDate.Date;
            Dictionary<string, TimeSpan> results = new Dictionary<string, TimeSpan>();

            DataTable data = new DataTable();
            data.Columns.Add("Date", typeof(DateTime));
            data.Columns.Add("Client", typeof(string));
            data.Columns.Add("Comments", typeof(string));
            data.Columns.Add("Hours", typeof(decimal));

            DataColumn[] primaryKey = new DataColumn[] { data.Columns[0], data.Columns[1], data.Columns[2] };
            data.PrimaryKey = primaryKey;

            string filename = String.Empty;
            while (dateIndex <= EndDate.Date)
            {
                filename = Constants.DataPath + dateIndex.ToString("yyyy-MM-dd") + ".xml";
                if (File.Exists(filename))
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(filename);

                    foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                    {
                        TimeSlice timeSlice = new TimeSlice(node);
                        DataRow row = data.Rows.Find(new object[] { dateIndex, timeSlice.Client, timeSlice.Notes });
                        decimal hours = 0;
                        if (row == null)
                        {
                            row = data.NewRow();
                            row["Client"] = timeSlice.Client;
                            row["Comments"] = timeSlice.Notes;
                            row["Date"] = timeSlice.StartTime.Date;
                            data.Rows.Add(row);
                        }
                        else
                        {
                            hours = Convert.ToDecimal(row[3]);
                        }

                        row["Hours"] = hours + (Convert.ToDecimal(timeSlice.EndTime.Subtract(timeSlice.StartTime).TotalHours) * (1 + Fluff));
                    }
                }

                dateIndex = dateIndex.AddDays(1);
            }

            StreamWriter writer = new StreamWriter(Filename);
            Utils.DataTableToCsv(writer, data, new Dictionary<string, string> { { "Hours", "{0:n1}" } });
            writer.Close();
        }

        #endregion
    }
}
