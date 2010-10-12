using System;
using System.Windows;

namespace GetBackToWork
{
    public partial class SaveReport : Window
    {
        public SaveReport()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;

            DateTime start = new DateTime(year, month, 1);
            DateTime end = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            Report report = new Report(DateTime.Now.ToString("yyyy-MM") + ".csv", start, end);

            report.Create();
        }
    }
}
