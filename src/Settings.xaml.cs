using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Xml;

namespace GetBackToWork
{
    public partial class Settings : Window
    {
        private ObservableCollection<string> Source
        {
            get;
            set;
        }

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

            Source = new ObservableCollection<string>(xml.DocumentElement.SelectNodes("Client").Cast<XmlNode>().Select(o => o.InnerText));

            ClientsListBox.Items.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
            ClientsListBox.ItemsSource = Source;

            XmlNode toastNode = xml.DocumentElement.SelectSingleNode("Toast");
            if (toastNode == null)
            {
                ToastEnabledCheckBox.IsChecked = true;
                ToastTimeComboBox.SelectedValue = 16;
            }
            else
            {
                ToastEnabledCheckBox.IsChecked = Convert.ToBoolean(toastNode.Attributes["enabled"].Value);
                for (int i = 1; i < 128; i *= 2)
                    ToastTimeComboBox.Items.Add(i);
                ToastTimeComboBox.SelectedValue = Convert.ToInt32(toastNode.Attributes["interval"].Value);
            }

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(Constants.SettingsPath);

            xml.DocumentElement.RemoveAll();

            foreach (string item in Source)
            {
                XmlNode node = xml.CreateElement("Client");
                node.InnerText = item.ToString();
                xml.DocumentElement.AppendChild(node);
            }

            XmlNode toastNode = xml.CreateElement("Toast");
            XmlAttribute toastEnabledAttrib = xml.CreateAttribute("enabled");
            toastEnabledAttrib.Value = Convert.ToString(ToastEnabledCheckBox.IsChecked);
            XmlAttribute toastTimeAttrib = xml.CreateAttribute("interval");
            toastTimeAttrib.Value = Convert.ToString(ToastTimeComboBox.SelectedValue);
            toastNode.Attributes.Append(toastEnabledAttrib);
            toastNode.Attributes.Append(toastTimeAttrib);
            xml.DocumentElement.AppendChild(toastNode);

            xml.Save(Constants.SettingsPath);

            IsDirty = false;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(ClientTextBox.Text) && !ClientsListBox.Items.Contains(ClientTextBox.Text))
            {
                Source.Add(ClientTextBox.Text);
                ClientTextBox.Clear();
                IsDirty = true;
            }
        }

        private void DeleteClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsListBox.SelectedIndex >= 0)
            {
                Source.Remove(ClientsListBox.SelectedValue.ToString());
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
