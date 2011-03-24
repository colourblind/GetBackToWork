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
        #region Properties

        private string HeaderTemplate { get; set; }
        private string ItemTemplate { get; set; }
        private string FooterTemplate { get; set; }

        private static IDictionary<string, ReportFormat> LoadedReports { get; set; }

        #endregion

        #region Constructors

        static ReportFormat()
        {
            LoadedReports = new Dictionary<string, ReportFormat>();
            XmlDocument xml = new XmlDocument();
            xml.Load(Constants.SettingsPath);

            XmlNode reports = xml.DocumentElement.SelectSingleNode("Reports");
            if (reports == null)
            {
                reports = xml.CreateElement("Reports");
                CreateDefaults(reports, xml);
                xml.DocumentElement.AppendChild(reports);
                xml.Save(Constants.SettingsPath);
            }

            foreach (XmlNode node in reports.SelectNodes("Report"))
                LoadedReports.Add(node.Attributes["name"].Value, new ReportFormat(node));
        }

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

        public ReportFormat(XmlNode node)
        {
            XmlNode headerNode = node.SelectSingleNode("HeaderTemplate");
            XmlNode itemNode = node.SelectSingleNode("ItemTemplate");
            XmlNode footerNode = node.SelectSingleNode("FooterTemplate");

            HeaderTemplate = headerNode == null ? String.Empty : headerNode.InnerText;
            ItemTemplate = itemNode.InnerText;
            FooterTemplate = footerNode == null ? String.Empty : footerNode.InnerText;
        }

        #endregion

        #region Methods

        public string Create(IEnumerable data)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(HeaderTemplate);

            foreach (object obj in data)
                builder.Append(NamedFormatter.Format(ItemTemplate, obj));

            builder.Append(FooterTemplate);
            return builder.ToString();
        }

        public static IEnumerable<string> GetReportNames()
        {
            return LoadedReports.Keys;
        }

        public static ReportFormat GetReportFormat(string name)
        {
            return LoadedReports[name];
        }

        public static void CreateDefaults(XmlNode parent, XmlDocument xml)
        {
            // CSV
            XmlNode csvReport = xml.CreateElement("Report");
            csvReport.Attributes.Append(xml.CreateAttribute("name"));
            csvReport.Attributes["name"].Value = "CSV";
            XmlNode csvFormat = xml.CreateElement("ItemTemplate");
            csvFormat.AppendChild(xml.CreateTextNode("\"{Client}\",\"{Comments}\",\"{Date:d}\",\"{Hours:f1}\"\r\n"));
            csvReport.AppendChild(csvFormat);
            parent.AppendChild(csvReport);

            // XML
            XmlNode xmlReport = xml.CreateElement("Report");
            xmlReport.Attributes.Append(xml.CreateAttribute("name"));
            xmlReport.Attributes["name"].Value = "XML";

            XmlNode xmlHeadFormat = xml.CreateElement("HeaderTemplate");
            xmlHeadFormat.AppendChild(xml.CreateTextNode("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Times>\r\n"));
            XmlNode xmlItemFormat = xml.CreateElement("ItemTemplate");
            xmlItemFormat.AppendChild(xml.CreateTextNode("   <Time client=\"{Client}\" comments=\"{Comments}\" date=\"{Date:d}\" hours=\"{Hours:f1}\" />\r\n"));
            XmlNode xmlFootFormat = xml.CreateElement("FooterTemplate");
            xmlFootFormat.AppendChild(xml.CreateTextNode("</Times>\r\n"));

            xmlReport.AppendChild(xmlHeadFormat);
            xmlReport.AppendChild(xmlItemFormat);
            xmlReport.AppendChild(xmlFootFormat);
            parent.AppendChild(xmlReport);

        }

        #endregion
    }
}
