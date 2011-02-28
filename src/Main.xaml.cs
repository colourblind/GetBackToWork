using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
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

        private bool IsStarted
        {
            get
            {
                return DateStarted != null;
            }
        }

        private DispatcherTimer ToastTimer
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
            systemTrayContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Settings", new EventHandler(Settings_Click)));
            systemTrayContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Exit", new EventHandler(Exit_Click)));

            SystemTrayIcon = new System.Windows.Forms.NotifyIcon();
            SystemTrayIcon.ContextMenu = systemTrayContextMenu;
            SystemTrayIcon.Visible = true;
            SystemTrayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
            SystemTrayIcon.Text = "Slacking off";
            SystemTrayIcon.BalloonTipTitle = "Get Back To Work";
            SystemTrayIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            SystemTrayIcon.Click += new EventHandler(SystemTrayIcon_Click);

            Hide();

            Left = System.Windows.SystemParameters.PrimaryScreenWidth - Width - 5;
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

            ClientComboBox.Items.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
            ClientComboBox.ItemsSource = xml.DocumentElement.SelectNodes("Client").Cast<XmlNode>().Select(o => o.InnerText);

            int toastMins = 15;
            bool toastEnabled = true;
            XmlNode toastNode = xml.DocumentElement.SelectSingleNode("Toast");
            if (toastNode != null)
            {
                toastEnabled = Convert.ToBoolean(toastNode.Attributes["enabled"].Value);
                toastMins = Convert.ToInt32(toastNode.Attributes["interval"].Value);
            }

            if (toastEnabled)
            {
                ToastTimer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
                ToastTimer.Tick += new EventHandler(ToastTimer_Tick);
                ToastTimer.Interval = new TimeSpan(0, toastMins, 0);
                ToastTimer.Start();
            }
        }

        private void Start()
        {
            DateStarted = DateTime.Now;
            GoButton.Content = "Stop";
            ClientComboBox.IsEnabled = false;
            SystemTrayIcon.Text = String.Format("{0} since {1}", ClientComboBox.SelectedValue, ((DateTime)DateStarted).ToString("hh:mm"));
            Hide();

            GoButton.Background = (System.Windows.Media.Brush)Application.Current.Resources["StopButtonBrush"];
        }

        private void Stop()
        {
            Stop(ClientComboBox.SelectedItem.ToString(), NotesTextBox.Text, (DateTime)DateStarted);
        }

        private void Stop(string client, string notes, DateTime dateStarted)
        {
            TimeSlice timeSlice = new TimeSlice(client, notes, dateStarted);
            timeSlice.Save();

            DateStarted = null;
            GoButton.Content = "Start";
            ClientComboBox.IsEnabled = true;
            NotesTextBox.Text = "";
            SystemTrayIcon.Text = "Slacking off";

            GoButton.Background = (System.Windows.Media.Brush)Application.Current.Resources["StartButtonBrush"];
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
            if (IsStarted)
                GoButton_Click(this, new RoutedEventArgs());
            Close();
        }

        private void Reports_Click(object sender, EventArgs e)
        {
            Hide();
            SaveReport saveReport = new SaveReport();
            saveReport.ShowDialog();
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            Hide();
            IsEnabled = false;

            Settings settings = new Settings();
            settings.ShowDialog();

            // Reload the client list and attempt to retain the selected value
            object selectedValue = ClientComboBox.SelectedValue;
            LoadSettings();
            ClientComboBox.SelectedValue = selectedValue;

            // If the item was removed, save the timeslice
            if (ClientComboBox.SelectedValue != selectedValue && IsStarted)
            {
                Stop(selectedValue.ToString(), NotesTextBox.Text, (DateTime)DateStarted);
                NotesTextBox.Text = "";
            }

            IsEnabled = true;
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
                if (IsStarted)
                    Stop();
                else
                    Start();
            }
        }

        void ToastTimer_Tick(object sender, EventArgs e)
        {
            SystemTrayIcon.BalloonTipText = "Currently on " + (IsStarted ? ClientComboBox.SelectedValue.ToString() : "NOTHING");
            SystemTrayIcon.ShowBalloonTip(4);
        }

        #endregion
    }
}
