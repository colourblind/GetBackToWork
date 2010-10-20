using System;
using System.Windows;
using System.Xml;

namespace GetBackToWork
{
    public partial class Settings : Window
    {
        private bool IsDirty
        {
            get;
            set;
        }

        public Settings()
        {
            InitializeComponent();

            IsDirty = false;

            XmlDocument xml = new XmlDocument();
            xml.Load(Constants.SettingsPath);

            ClientsListBox.Items.Clear();
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Client"))
                ClientsListBox.Items.Add(node.InnerText);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(Constants.SettingsPath);

            xml.DocumentElement.RemoveAll();

            foreach (object item in ClientsListBox.Items)
            {
                XmlNode node = xml.CreateElement("Client");
                node.InnerText = item.ToString();
                xml.DocumentElement.AppendChild(node);
            }

            xml.Save(Constants.SettingsPath);

            IsDirty = false;

            Close();
        }

        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(ClientTextBox.Text) && !ClientsListBox.Items.Contains(ClientTextBox.Text))
            {
                ClientsListBox.Items.Add(ClientTextBox.Text);
                ClientTextBox.Clear();
                IsDirty = true;
            }
        }

        private void DeleteClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsListBox.SelectedIndex >= 0)
            {
                ClientsListBox.Items.RemoveAt(ClientsListBox.SelectedIndex);
                IsDirty = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsDirty)
                e.Cancel = MessageBox.Show("You have unsaved changes! Press OK to abandon the changes, or cancel to return to the settings window", "Abandon changes?", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Cancel;
        }
    }
}
