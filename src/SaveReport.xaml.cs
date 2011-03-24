using System;
using System.Windows;
using System.Windows.Controls;

namespace GetBackToWork
{
    public partial class SaveReport : Window
    {
        public SaveReport()
        {
            InitializeComponent();

            DateTime now = DateTime.Now;
            StartDatePicker.SelectedDate = new DateTime(now.Year, now.Month, 1);
            EndDatePicker.SelectedDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
            
            foreach (string name in ReportFormat.GetReportNames())
                FormatListBox.Items.Add(name);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(FilenameTextBox.Text))
            {
                MessageBox.Show("Please select where to save your report", "Where do you want to save today?", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Report report = new Report(FilenameTextBox.Text, (DateTime)StartDatePicker.SelectedDate, (DateTime)EndDatePicker.SelectedDate);
                report.Fluff = Convert.ToDecimal(FluffSlider.Value / 100);

                report.Create();

                Close();
            }
        }

        private void StartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
                EndDatePicker.SelectedDate = StartDatePicker.SelectedDate;
        }

        private void EndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EndDatePicker.SelectedDate < StartDatePicker.SelectedDate)
                StartDatePicker.SelectedDate = EndDatePicker.SelectedDate;
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog fileDialog = new System.Windows.Forms.SaveFileDialog();
            fileDialog.FileName = DateTime.Now.ToString("yyyy-MM") + ".csv";
            fileDialog.AddExtension = true;
            fileDialog.OverwritePrompt = true;
            fileDialog.RestoreDirectory = false;
            fileDialog.Filter = "CSV (*.csv)|*.csv|All files (*.*)|*.*";

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                FilenameTextBox.Text = fileDialog.FileName;
        }
    }
}
