using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
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

            List<object> postMungeData = new List<object>();
            foreach (DataRow row in data.Rows)
                postMungeData.Add(new { Client = row["Client"], Comments = row["Comments"], Date = row["Date"], Hours = row["Hours"] });

            ReportFormat reportFormat = new ReportFormat("\"{Client}\",\"{Comments}\",\"{Date:d}\",\"{Hours:f1}\"\r\n");
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(Filename);
                writer.Write(reportFormat.Create(postMungeData));
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        #endregion
    }

    class ReportFormat
    {
        private string HeaderTemplate { get; set; }
        private string ItemTemplate { get; set; }
        private string FooterTemplate { get; set; }

        public ReportFormat(string itemTemplate)
            : this("", itemTemplate, "")
        {

        }

        public ReportFormat(string headerTemplate, string itemTemplate, string footerTemplate)
        {
            HeaderTemplate = headerTemplate;
            ItemTemplate = itemTemplate;
            FooterTemplate = footerTemplate;
        }

        public string Create(IEnumerable data)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(HeaderTemplate);

            foreach (object obj in data)
                builder.Append(NamedFormatter.Format(ItemTemplate, obj));

            builder.Append(FooterTemplate);
            return builder.ToString();
        }
    }
}
