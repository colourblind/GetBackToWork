using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Xml;

namespace GetBackToWork
{
    public partial class Main : Window
    {
        #region Properties

        private System.Windows.Forms.NotifyIcon SystemTrayIcon
        {
            get;
            set;
        }

        private DateTime? DateStarted
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        public Main()
        {
            InitializeComponent();

            System.Windows.Forms.ContextMenu systemTrayContextMenu = new System.Windows.Forms.ContextMenu();
            systemTrayContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Reports", new EventHandler(Reports_Click)));
            systemTrayContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Exit", new EventHandler(Exit_Click)));

            SystemTrayIcon = new System.Windows.Forms.NotifyIcon();
            SystemTrayIcon.ContextMenu = systemTrayContextMenu;
            SystemTrayIcon.Visible = true;
            SystemTrayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
            SystemTrayIcon.Text = "Slacking off";
            SystemTrayIcon.Click += new EventHandler(SystemTrayIcon_Click);

            Hide();

            Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - Width - 5;
            Top = 5;

            DateStarted = null;
            LoadSettings();
        }

        #endregion

        #region Methods

        private void LoadSettings()
        {
            if (!Directory.Exists(Constants.DataPath))
                Directory.CreateDirectory(Constants.DataPath);

            if (!File.Exists(Constants.SettingsPath))
            {
                StreamWriter writer = new StreamWriter(File.Create(Constants.SettingsPath));
                writer.WriteLine("<GetBackToWork></GetBackToWork>");
                writer.Close();
            }

            XmlDocument xml = new XmlDocument();
            xml.Load(Constants.SettingsPath);

            ClientComboBox.Items.Clear();
            foreach (XmlNode node in xml.DocumentElement.SelectNodes("Client"))
                ClientComboBox.Items.Add(node.InnerText);
        }

        #endregion

        #region Event Handlers

        void SystemTrayIcon_Click(object sender, EventArgs e)
        {
            Show();
            Activate();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            // Finish off the current timeslice if the window is closed before stop is clicked
            if (DateStarted != null)
                GoButton_Click(this, new RoutedEventArgs());
            Close();
        }

        private void Reports_Click(object sender, EventArgs e)
        {
            SaveReport saveReport = new SaveReport();
            saveReport.ShowDialog();
            Hide();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SystemTrayIcon.Dispose();
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientComboBox.Items.Count == 0)
                MessageBox.Show("Before you can track time you need to create a client. Right click the icon in the system tray and select 'Settings'.", "Client list empty", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else if (ClientComboBox.SelectedIndex < 0)
                MessageBox.Show("You must select a client before tracking time", "Please select a client", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else
            {
                if (DateStarted == null)
                {
                    DateStarted = DateTime.Now;
                    GoButton.Content = "Stop";
                    ClientComboBox.IsEnabled = false;
                    SystemTrayIcon.Text = String.Format("{0} since {1}", ClientComboBox.SelectedValue, ((DateTime)DateStarted).ToString("hh:mm"));
                    Hide();
                }
                else
                {
                    TimeSlice timeSlice = new TimeSlice(ClientComboBox.SelectedItem.ToString(), NotesTextBox.Text, (DateTime)DateStarted);
                    timeSlice.Save();

                    DateStarted = null;
                    GoButton.Content = "Start";
                    ClientComboBox.IsEnabled = true;
                    NotesTextBox.Text = "";
                    SystemTrayIcon.Text = "Slacking off";
                }
            }
        }

        #endregion
    }
}
