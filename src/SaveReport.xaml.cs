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
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Report report = new Report(DateTime.Now.ToString("yyyy-MM") + ".csv", (DateTime)StartDatePicker.SelectedDate, (DateTime)EndDatePicker.SelectedDate);

            report.Create();
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
    }
}
