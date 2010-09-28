using System;
using System.IO;
using System.Text;
using System.Xml;

namespace GetBackToWork
{
    public class TimeSlice
    {
        #region Properties

        public string Client
        {
            get;
            set;
        }

        public string Notes
        {
            get;
            set;
        }

        public DateTime StartTime
        {
            get;
            set;
        }

        public DateTime EndTime
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public TimeSlice() : this("", "", DateTime.Now)
        {

        }

        public TimeSlice(string client, string notes, DateTime startTime)
            : this(client, notes, startTime, DateTime.Now)
        {

        }

        public TimeSlice(string client, string notes, DateTime startTime, DateTime endTime)
        {
            Client = client;
            Notes = notes;
            StartTime = startTime;
            EndTime = endTime;
        }

        public TimeSlice(XmlNode element)
        {
            Client = element.SelectSingleNode("Client").InnerText;
            Notes = element.SelectSingleNode("Notes").InnerText;
            StartTime = Convert.ToDateTime(element.SelectSingleNode("StartTime").InnerText);
            EndTime = Convert.ToDateTime(element.SelectSingleNode("EndTime").InnerText);
        }

        #endregion

        #region Methods

        public void Save()
        {
            string filename = Constants.DataPath + StartTime.ToString("yyyy-MM-dd") + ".xml";

            if (!File.Exists(filename))
            {
                StreamWriter writer = new StreamWriter(File.Create(filename));
                writer.WriteLine("<GetBackToWork></GetBackToWork>");
                writer.Close();
            }

            XmlDocument xml = new XmlDocument();
            xml.Load(filename);
            xml.DocumentElement.AppendChild(CreateXmlNode(xml));
            xml.Save(filename);
        }

        private XmlNode CreateXmlNode(XmlDocument xml)
        {
            XmlNode node = xml.CreateElement("TimeSlice");

            XmlNode clientNode = xml.CreateElement("Client");
            clientNode.InnerText = Client;

            XmlNode notesNode = xml.CreateElement("Notes");
            notesNode.InnerText = Notes;

            XmlNode startTimeNode = xml.CreateElement("StartTime");
            startTimeNode.InnerText = StartTime.ToString();

            XmlNode endTimeNode = xml.CreateElement("EndTime");
            endTimeNode.InnerText = EndTime.ToString();

            node.AppendChild(clientNode);
            node.AppendChild(notesNode);
            node.AppendChild(startTimeNode);
            node.AppendChild(endTimeNode);

            return node;
        }

        #endregion
    }
}
