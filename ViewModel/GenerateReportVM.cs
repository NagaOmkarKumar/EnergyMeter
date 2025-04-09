using Microsoft.Win32;
using MimeKit;
using Project_K.Model;
using Project_K.ViewModel.Commands;
using SQLite;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using MailKit.Net.Smtp;
using System.Timers;
using MailKit.Security;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System.Windows.Threading;
using System.Diagnostics.CodeAnalysis;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.ObjectModel;
using System.Data.Entity;
using Org.BouncyCastle.Crypto.Macs;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
//using CommunityToolkit.Mvvm.Input;
using System.Net.NetworkInformation;

namespace Project_K.ViewModel
{
    public class GenerateReportVM : INotifyPropertyChanged
    {
        private DateTime? fromDate;
        private DateTime? toDate;
        private string _loc;
        private IEnumerable<string> meters;
        private string selectedMeter;
        private DispatcherTimer _emailTimer;
        public ObservableCollection<string> EmailAddresses { get; set; } = new ObservableCollection<string>();
        public DateTime? FromDate
        {
            get { return fromDate; }
            set
            {
                fromDate = value;
                OnPropertyChanged("FromDate");
            }
        }
        public DateTime? ToDate
        {
            get { return toDate; }
            set
            {
                toDate = value;
                OnPropertyChanged("ToDate");
            }
        }
        private string _emailAddress;
        public string EmailAddress
        {
            get => _emailAddress;
            set
            {
                _emailAddress = value;
                OnPropertyChanged(nameof(EmailAddress));
            }
        }
        public string Loc
        {
            get => _loc;
            set
            {
                _loc = value;
                OnPropertyChanged(nameof(Loc));
            }
        }
        public IEnumerable<string> Meters
        {
            get => meters;
            set
            {
                if (meters != value)
                {
                    meters = value;
                    OnPropertyChanged(nameof(Meters));
                }
            }
        }
        public string SelectedMeter
        {
            get => selectedMeter;
            set
            {
                selectedMeter = value;
                OnPropertyChanged(nameof(SelectedMeter));
            }
        }
        public ICommand DownloadCommand => new RelayCommand(DownloadReport);
        public ICommand BrowseCommand => new RelayCommand(BrowsePath);
        public ICommand SaveCommand => new RelayCommand(savePath);
        public ICommand AddCommand => new RelayCommand(AddEmail);
        public ICommand RemoveCommand => new RelayCommand(RemoveEmail);
        public List<string> Classes { get; set; }
        public string SelectedClass { get; set; }

        public GenerateReportVM()
        {
            Classes = new List<string> { "MeterData", "MinMaxValues", "EnergyData" };
            Meters = GetMeterNames();
            EmailAddresses = new ObservableCollection<string>();
            //LoadEmailAddresses();
            if (Meters == null || !Meters.Any())
            {
                SelectedMeter = "SelectMeter"; // Or handle appropriately 
            }
            else
            {
                // SelectedMeter = Meters.First();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddEmail()
        {
            if (!string.IsNullOrWhiteSpace(EmailAddress) && !EmailAddresses.Contains(EmailAddress))
            {
                // Trace.WriteLine(EmailAddress.Length);
                SQLiteConnection connection = new SQLiteConnection(App.databasePath);
                //connection.DeleteAll<FilePath>();
                FilePath file = new FilePath()
                {
                    EmailAddress = EmailAddress,
                };
                connection.Insert(file);
                //EmailAddresses.Clear();
                EmailAddresses.Add(EmailAddress);

                List<string> emails = connection.Table<FilePath>().Select(v => v.EmailAddress).Distinct().ToList();

                foreach (var email in emails)
                {
                    // Trace.WriteLine(email);
                }
            }
        }

        private void RemoveEmail()
        {
            if (EmailAddresses.Contains(EmailAddress))
            {
                SQLiteConnection connection = new SQLiteConnection(App.databasePath);

                var email = connection.Table<FilePath>().FirstOrDefault(f => f.EmailAddress == EmailAddress);
                if (email != null)
                {
                    connection.Delete(email);
                    EmailAddresses.Remove(EmailAddress);

                    List<string> emails = connection.Table<FilePath>().Select(v => v.EmailAddress).Distinct().ToList();

                    foreach (var emailid in emails)
                    {
                        // Trace.WriteLine(emailid);
                    }
                }

            }
        }

        private void LoadEmailAddresses()
        {

            SQLiteConnection connection = new SQLiteConnection(App.databasePath);
            List<string> emails = connection.Table<FilePath>().Select(v => v.EmailAddress).Distinct().ToList();
            EmailAddresses.Clear();
            //Trace.WriteLine("////////////");
            foreach (var email in emails)
            {
                if (!string.IsNullOrEmpty(email))
                {
                    // Trace.WriteLine(email.Length);
                    EmailAddresses.Add(email);
                    // Trace.WriteLine(email);
                }

            }
            //Trace.WriteLine("////////////");
        }
        public string[] GetEmailAddressesArray()
        {
            return EmailAddresses.ToArray();
        }

        public List<string> GetMeterNames()
        {
            var meterNames = new List<string>();
            try
            {
                using (var connection = new SQLiteConnection(App.databasePath))
                {
                    var meterDetailsList = connection.Table<MeterDetails>().ToList();
                    var validMeterNames = meterDetailsList.Where(m => !string.IsNullOrWhiteSpace(m.MeterName)).Select(m => m.MeterName).ToList();
                    meterNames.AddRange(validMeterNames);
                }
            }
            catch (Exception ex)
            {
                //Trace.WriteLine($"Error fetching meter names: {ex.Message}");
            }
            return meterNames;
        }

        private string GetFileName()
        {
            string qu1 = "";
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
                {
                    var query = connection.Table<FilePath>().FirstOrDefault();

                    if (query != null)
                    {
                        qu1 = query.Fileurl;
                        //string filename = System.IO.Path.GetFileName(FileUrl.LocalPath)
                    }
                    else
                    {
                        qu1 = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    }
                }
            }
            catch (Exception ex)
            {
                //Trace.WriteLine(ex.Message);
            }
            return qu1;
        }

        private void DownloadReport()
        {
            FileStream stream = null;
            StreamWriter writer = null;
            FileStream stream1 = null;
            StreamWriter writer1 = null;
            FileStream stream2 = null;
            StreamWriter writer2 = null;
            string selectedClass = SelectedClass;

            try
            {
                if (selectedClass == "MeterData")
                {
                    int success = Validate(FromDate.Value.ToString(), ToDate.Value.ToString());
                    string qu = "";
                    string qu1 = GetFileName();

                    SQLiteConnection connection = new SQLiteConnection(App.databasePath);

                    string fileName1 = qu1 + "\\";
                    string filename3 = "MeterDataReport" + FromDate.Value.ToString("dd-MM-yy") + "-" + ToDate.Value.ToString("dd-MM-yy") + DateTime.Now.ToString("HH-mm");
                    string filename4 = ".csv";
                    string fileName = fileName1 + filename3 + filename4;

                    stream = new FileStream(fileName, FileMode.OpenOrCreate);
                    stream.Seek(0, SeekOrigin.End);
                    writer = new StreamWriter(stream, Encoding.UTF8);
                    { writer.WriteLine("METERNAME,DATE,TIME,VOLTAGE,CURRENT,PF,Energy"); }

                    if (success >= 0)
                    {
                        List<DateTime> queryDates = GetDatesBetween(FromDate.Value, ToDate.Value);
                        //query_return_values.Clear();
                        try
                        {
                            foreach (DateTime date in queryDates)
                            {
                                qu = date.ToString("dd-MM-yyyy");
                                var q = connection.Table<MeterData>().Where(v => v.Date.StartsWith(qu));
                                foreach (MeterData report in q)

                                {
                                    if (report != null)
                                    {
                                        writer.WriteLine($"{report.MeterName},{'*' + report.Date},{'*' + report.Timestamp},{report.Voltage},{report.Current},{report.PF},{report.KWH}");
                                    }
                                }
                            }
                            MessageBox.Show($"Report saved to {fileName}", "Report Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            //Trace.WriteLine(ex.Message);
                            MessageBox.Show(ex.Message, "Error writing CSV", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else if (selectedClass == "MinMaxValues")
                {
                    int success = Validate(FromDate.Value.ToString(), ToDate.Value.ToString());
                    string qu = "";
                    string qu1 = GetFileName();
                    SQLiteConnection connection = new SQLiteConnection(App.databasePath);

                    string fileName1 = qu1 + "\\";
                    string filename5 = "MinMaxReport - " + FromDate.Value.ToString("dd-MM-yy") + "-" + ToDate.Value.ToString("dd-MM-yy") + DateTime.Now.ToString("HH-mm");
                    string filename4 = ".csv";
                    string fileName0 = fileName1 + filename5 + filename4;


                    stream1 = new FileStream(fileName0, FileMode.OpenOrCreate);
                    stream1.Seek(0, SeekOrigin.End);
                    writer1 = new StreamWriter(stream1, Encoding.UTF8);
                    { writer1.WriteLine("METERNAME,DATE,MAXVOLTAGE,TIME,MAXCURRENT,TIME,MAXPF,TIME,MINVOLTAGE,TIME,MINCURRENT,TIME,MINPF,TIME,Energy"); }
                    if (success >= 0)
                    {
                        List<DateTime> queryDates = GetDatesBetween(FromDate.Value, ToDate.Value);
                        //query_return_values.Clear();
                        try
                        {
                            foreach (DateTime date in queryDates)
                            {
                                qu = date.ToString("dd-MM-yyyy");
                                var q = connection.Table<MinMaxValues>().Where(v => v.Date.StartsWith(qu));
                                foreach (MinMaxValues report in q)
                                {
                                    if (report != null && report.MaxEnergy != 0)
                                    {
                                        writer1.WriteLine($"{report.MeterName},{report.Date},{report.MaxVoltage},{report.MaxVoltageTime},{report.MaxCurrent},{report.MaxCurrentTime},{report.MaxPF},{report.MaxPFTime},{report.MinVoltage},{report.MinVoltageTime},{report.MinCurrent},{report.MinCurrentTime},{report.MinPF},{report.MinPFTime},{report.MaxEnergy}");
                                    }
                                }
                            }
                            MessageBox.Show($"Report saved to {fileName0}", "Report Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            // Trace.WriteLine(ex.Message);
                            MessageBox.Show(ex.Message, "Error writing CSV", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else if (selectedClass == "EnergyData")
                {
                    int success = Validate(FromDate.Value.ToString(), ToDate.Value.ToString());
                    SQLiteConnection connection = new SQLiteConnection(App.databasePath);
                    string fileName1 = GetFileName() + "\\";
                    string filename5 = "EnergyDataReport - " + FromDate.Value.ToString("dd-MM-yy") + "_" + ToDate.Value.ToString("dd-MM-yy") + "_" + DateTime.Now.ToString("HH-mm");
                    string filename4 = ".csv";
                    string fileName0 = fileName1 + filename5 + filename4;
                    stream2 = new FileStream(fileName0, FileMode.OpenOrCreate);
                    stream2.Seek(0, SeekOrigin.End);
                    writer2 = new StreamWriter(stream2, Encoding.UTF8);
                    { writer2.WriteLine("METERNAME,DATE,Energy"); }

                    if (success >= 0)
                    {
                        List<DateTime> queryDates = GetDatesBetween(FromDate.Value, ToDate.Value);
                        //query_return_values.Clear();
                        try
                        {
                            foreach (DateTime date in queryDates)
                            {
                                string qu = date.ToString("dd-MM-yyyy");
                                var energyDataList = new List<EnergyData>();
                                try
                                {
                                    MinMaxValues weldreport = connection.Table<MinMaxValues>()
                                                    .Where(d => d.MeterName == "Welding1" && d.Date == qu)
                                                    .OrderByDescending(d => d.Timestamp).FirstOrDefault();
                                    double? weldkwh = weldreport?.MaxEnergy;
                                    double? compresskwh = connection.Table<MinMaxValues>()
                                                        .Where(d => d.MeterName == "Compressor" && d.Date == qu)
                                                        .OrderByDescending(d => d.Timestamp).FirstOrDefault()?.MaxEnergy;
                                    double? weld2kwh = connection.Table<MinMaxValues>()
                                                        .Where(d => d.MeterName == "Welding2" && d.Date == qu)
                                                        .OrderByDescending(d => d.Timestamp).FirstOrDefault()?.MaxEnergy;
                                    MinMaxValues bracereport = connection.Table<MinMaxValues>()
                                                        .Where(d => d.MeterName == "Brace" && d.Date == qu)
                                                        .OrderByDescending(d => d.Timestamp).FirstOrDefault();
                                    double? bracekwh = bracereport?.MaxEnergy;
                                    double? robotkwh = connection.Table<MinMaxValues>()
                                                        .Where(d => d.MeterName == "Robot" && d.Date == qu)
                                                        .OrderByDescending(d => d.Timestamp).FirstOrDefault()?.MaxEnergy;
                                    double? weld3kwh = connection.Table<MinMaxValues>()
                                    .Where(d => d.MeterName == "Welding3" && d.Date == qu)
                                    .OrderByDescending(d => d.Timestamp).FirstOrDefault()?.MaxEnergy;
                                    double? ffkwh = connection.Table<MinMaxValues>()
                                                        .Where(d => d.MeterName == "FuelFiller" && d.Date == qu)
                                                        .OrderByDescending(d => d.Timestamp).FirstOrDefault()?.MaxEnergy;
                                    double? mainkwh = connection.Table<MinMaxValues>()
                                                .Where(d => d.MeterName == "Main" && d.Date == qu)
                                                .OrderByDescending(d => d.Timestamp)
                                                .Select(d => d.MaxEnergy) // Selects the MaxEnergy directly
                                                .FirstOrDefault();

                                    // double Welding1KWh = weldkwh - compresskwh;
                                    // Trace.WriteLine(Welding1KWh);
                                    //double Welding2KWh = weld2kwh - (bracekwh + robotkwh);
                                    //Trace.WriteLine(Welding2KWh);

                                    double Welding1KWh = weldkwh.HasValue ? (weldkwh.Value - (compresskwh ?? 0)) : 1;
                                    //Trace.WriteLine(Welding1KWh);
                                    double Welding2KWh = weld2kwh.HasValue ? (weld2kwh.Value - ((bracekwh ?? 0) + (robotkwh ?? 0))) : 1;
                                    //Trace.WriteLine(Welding2KWh);
                                    double compresskwhValue = compresskwh.HasValue ? compresskwh.Value : 1;
                                    double bracekwhValue = bracekwh.HasValue ? bracekwh.Value : 1;
                                    double robotkwhValue = robotkwh.HasValue ? robotkwh.Value : 1;
                                    double ffkwhValue = ffkwh.HasValue ? ffkwh.Value : 1;
                                    double mainkwhValue = mainkwh.HasValue ? mainkwh.Value : 1;
                                    double Welding3kwhValue = weld3kwh.HasValue ? weld3kwh.Value : 1;

                                    energyDataList.Add(new EnergyData { MeterName = "Welding1", KWH = Welding1KWh, Date = qu });
                                    energyDataList.Add(new EnergyData { MeterName = "Compressor", KWH = compresskwhValue, Date = qu });
                                    energyDataList.Add(new EnergyData { MeterName = "Welding2", KWH = Welding2KWh, Date = qu });
                                    energyDataList.Add(new EnergyData { MeterName = "Brace", KWH = bracekwhValue, Date = qu });
                                    energyDataList.Add(new EnergyData { MeterName = "Robot", KWH = robotkwhValue, Date = qu });
                                    energyDataList.Add(new EnergyData { MeterName = "FuelFiller", KWH = ffkwhValue, Date = qu });
                                    energyDataList.Add(new EnergyData { MeterName = "Main", KWH = mainkwhValue, Date = qu });
                                    energyDataList.Add(new EnergyData { MeterName = "Welding3", KWH = Welding3kwhValue, Date = qu });
                                    //Trace.WriteLine(Welding1KWh, qu);
                                    //Trace.WriteLine(Welding2KWh, qu);
                                    //Trace.WriteLine(compresskwhValue, qu);
                                    //Trace.WriteLine(bracekwhValue, qu);
                                    //Trace.WriteLine(robotkwhValue, qu);
                                    //Trace.WriteLine(ffkwhValue, qu);
                                    //Trace.WriteLine(mainkwhValue, qu);

                                    //var q = connection.Table<EnergyData>().Where(v => v.Date.StartsWith(qu));
                                }
                                catch (Exception ex)
                                {
                                    // Handle any exceptions related to data processing
                                    // Trace.WriteLine($"Error processing data for {qu}: {ex.Message}");
                                }
                                foreach (var energyData in energyDataList)
                                {
                                    //Trace.WriteLine("writing");
                                    if (energyData.KWH != 0)
                                    {
                                        writer2.WriteLine($"{energyData.Date},{energyData.MeterName},{energyData.KWH}");
                                        //Trace.WriteLine("writing11");
                                    }

                                }

                            }
                            MessageBox.Show($"Report saved to {fileName0}", "Report Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            // Trace.WriteLine(ex.Message);
                            MessageBox.Show(ex.Message, "Error writing CSV", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }

            catch { }

            finally
            {
                if (writer != null)
                {
                    writer.Dispose();
                }
                if (stream != null)
                {
                    stream.Dispose();
                }
                if (writer1 != null)
                {
                    writer1.Dispose();
                }
                if (stream1 != null)
                {
                    stream1.Dispose();
                }
                if (writer2 != null)
                {
                    writer2.Dispose();
                }
                if (stream2 != null)
                {
                    stream2.Dispose();
                }
            }
        }

        private static List<DateTime> GetDatesBetween(DateTime date1, DateTime date2)
        {
            List<DateTime> allDates = new List<DateTime>();

            for (DateTime date = date1; date <= date2; date = date.AddDays(1))
            {
                allDates.Add(date.Date);
            }
            return allDates;
        }

        private int Validate(string date1, string date2)
        {
            if (!string.IsNullOrEmpty(date1) && !string.IsNullOrEmpty(date2))
            {
                DateTime Date1 = DateTime.Parse(date1);
                DateTime Date2 = DateTime.Parse(date2);
                int value = Date2.CompareTo(Date1);

                if (value < 0)
                {
                    MessageBox.Show("From Date should be earlier than To Date. Please select again", "Invalid Date Range", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return value;
            }
            else
            {
                MessageBox.Show("Date should not be empty. Please select again", "Empty Date", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
        }

        private void BrowsePath()
        {
            var dialog = new OpenFolderDialog()
            {
                //Title = "EnergyMeter",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                Multiselect = true
            };

            string folderName = "";
            if (dialog.ShowDialog() == true)
            {
                folderName = dialog.FolderName;
                Loc = folderName;
            }
        }

        private void savePath()
        {
            if (string.IsNullOrEmpty(Loc))
            {
                MessageBox.Show("Please select a folder first.");
                return;
            }
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
                {
                    connection.Execute("DELETE FROM FilePath");
                    FilePath file = new FilePath()
                    {
                        //MeterName = SelectedMeter,
                        Fileurl = Loc,
                        TimeStamp = DateTime.Now.ToString("dd-MM-yyyy HH_mm"),
                    };
                    connection.Insert(file);
                }
                MessageBox.Show("Document location stored successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void DownloadReportForPreviousDay1()
        {
            string fileName1 = GetFileName() + "\\";
            string filename3 = "MeterData" + DateTime.Now.AddDays(-1).ToString("dd-MM-yy");
            string filename4 = ".csv";
            string fileName = fileName1 + filename3 + filename4;

            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("METERNAME,DATE,TIME,VOLTAGE,CURRENT,PF,Energy,ITHD%");

            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                DateTime startTime = DateTime.Now.AddDays(-1).Date.AddHours(6); // Previous day's 6 AM
                DateTime endTime = DateTime.Now.Date.AddHours(6); // Today's 6 AM

                string startTimeStr = startTime.ToString("HH:mm:ss");
                string endTimeStr = endTime.ToString("HH:mm:ss");
                string startDateStr = startTime.ToString("dd-MM-yyyy");
                string endDateStr = endTime.ToString("dd-MM-yyyy");

                var query = connection.Table<MeterData>()
                             .Where(v =>
                                   (string.Compare(v.Date, startDateStr) > 0 ||
                                    (v.Date == startDateStr && string.Compare(v.Timestamp, startTimeStr) >= 0)) &&
                                   (string.Compare(v.Date, endDateStr) < 0 ||
                                    (v.Date == endDateStr && string.Compare(v.Timestamp, endTimeStr) < 0)));

                // string queryDate = DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy");
                //var query = connection.Table<MeterData>().Where(v => v.Date.StartsWith(queryDate));
                foreach (MeterData report in query)
                {
                    if (report != null)
                    {
                        // Append to StringBuilder instead of writing to file directly
                        csvContent.AppendLine($"{report.MeterName},{'*' + report.Date},{'*' + report.Timestamp},{report.Voltage},{report.Current},{report.PF},{report.KWH},{report.KVA}");
                    }
                }
            }

            try
            {
                File.WriteAllText(fileName, csvContent.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                //Trace.WriteLine("Error writing CSV: " + ex.Message);
                // MessageBox.Show(ex.Message, "Error writing CSV", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteOldEntries(DateTime currentDate)
        {

            using (var connection = new SQLiteConnection(App.databasePath))
            {
                DateTime startTime = DateTime.Now.AddDays(-1).Date.AddHours(6); // Previous day's 6 AM
                DateTime endTime = DateTime.Now.Date.AddHours(6); // Today's 6 AM

                string startTimeStr = startTime.ToString("HH:mm:ss");
                string endTimeStr = endTime.ToString("HH:mm:ss");
                string startDateStr = startTime.ToString("dd-MM-yyyy");
                string endDateStr = endTime.ToString("dd-MM-yyyy");

                var query = connection.Table<MeterData>()
                             .Where(v =>
                                   (string.Compare(v.Date, startDateStr) > 0 ||
                                    (v.Date == startDateStr && string.Compare(v.Timestamp, startTimeStr) >= 0)) &&
                                   (string.Compare(v.Date, endDateStr) < 0 ||
                                    (v.Date == endDateStr && string.Compare(v.Timestamp, endTimeStr) < 0)));
                //string now = currentDate.ToString("dd-MM-yyyy");

                // Delete entries older than the current date
                // var query = connection.Table<MeterData>().Where(d => d.Date == now);

                foreach (MeterData report in query)
                {
                    connection.Delete(report);
                }
                //Trace.WriteLine("Deleted old entries from MeterData.");

            }
        }

        public void EnergyDataReport()
        {
            string fileName1 = GetFileName() + "\\";
            string filename3 = "EnergyData_Report_" + DateTime.Now.AddDays(-1).ToString("dd-MM-yy");
            string filename4 = ".csv";
            string fileName = fileName1 + filename3 + filename4;
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("METERNAME,DATE,KWH");
            // Trace.WriteLine("EnergyData");
            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                string queryDate = DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy");
                var query = connection.Table<EnergyData>().Where(v => v.Date.StartsWith(queryDate));
                if (query != null)
                {
                    foreach (EnergyData report in query)
                    {
                        if (report != null)
                        {
                            // Append to StringBuilder
                            csvContent.AppendLine($"{report.MeterName},{report.Date},{report.KWH}");
                            // Trace.WriteLine("EnergyValues");
                        }
                    }
                }
            }

            try
            {
                File.WriteAllText(fileName, csvContent.ToString(), Encoding.UTF8);
                // Trace.WriteLine("EnergyData Done");
            }
            catch (Exception ex)
            {
                //Trace.WriteLine("Error writing CSV: " + ex.Message);
                // MessageBox.Show(ex.Message, "Error writing CSV", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadMinmaxReportForPreviousDay()
        {
            string fileName1 = GetFileName() + "\\";
            string filename3 = "MinMax_Report_" + DateTime.Now.AddDays(-1).ToString("dd-MM-yy");
            string filename4 = ".csv";
            string fileName = fileName1 + filename3 + filename4;


            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("METERNAME,DATE,MAXVOLTAGE,TIME,MAXCURRENT,TIME,MAXPF,TIME,MINVOLTAGE,TIME,MINCURRENT,TIME,MINPF,TIME,ENERGY");

            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                string queryDate = DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy");
                var query = connection.Table<MinMaxValues>().Where(v => v.Date.StartsWith(queryDate));
                if (query != null)
                {
                    foreach (MinMaxValues report in query)
                    {
                        if (report != null && report.MaxEnergy != 0)
                        {
                            csvContent.AppendLine($"{report.MeterName},{'*' + report.Date},{report.MaxVoltage},{'*' + report.MaxVoltageTime},{report.MaxCurrent},{'*' + report.MaxCurrentTime},{report.MaxPF},{'*' + report.MaxPFTime},{report.MinVoltage},{'*' + report.MinVoltageTime},{report.MinCurrent},{'*' + report.MinCurrentTime},{report.MinPF},{'*' + report.MinPFTime},{report.MaxEnergy}");
                        }
                    }
                }
            }

            // Write all collected data to the file at once
            try
            {
                File.WriteAllText(fileName, csvContent.ToString(), Encoding.UTF8);
                // Trace.WriteLine("Downloaded");
            }
            catch (Exception ex)
            {
                // Trace.WriteLine("Error writing CSV: " + ex.Message);
                // MessageBox.Show(ex.Message, "Error writing CSV", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task SendDailyEmail3()
        {
            SQLiteConnection connection = new SQLiteConnection(App.databasePath);
            DownloadMinmaxReportForPreviousDay();
            LoadEmailAddresses();
            string[] emailAddressesArray = GetEmailAddressesArray();

            string fileName = Path.Combine(GetFileName() + "\\", "MinMax_Report_" + DateTime.Now.AddDays(-1).ToString("dd-MM-yy") + ".csv");
            string fileName1 = Path.Combine(GetFileName() + "\\", "EnergyData_Report_" + DateTime.Now.AddDays(-1).ToString("dd-MM-yy") + ".csv");
            // Trace.WriteLine("Email");
            if (!File.Exists(fileName) || !File.Exists(fileName1))
            {
                //MessageBox.Show("Report file does not exist. Cannot send email.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string formattedEmails = "{" + string.Join(", ", emailAddressesArray.Select(email => $"\"{email}\"")) + "}";
            List<string> toAddresses2 = connection.Table<FilePath>().Select(v => v.EmailAddress).Distinct().ToList();
            List<string> toAddresses1 = new List<string> { "roboworksautomation@gmail.com", "nattinagakumar@gmail.com", "nagakumar_011@yahoo.com" };
            List<string> toAddresses = emailAddressesArray.ToList();
            if (toAddresses.Count == 0)
            {
                // MessageBox.Show("No email addresses to send the report to.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            foreach (string toAddress in toAddresses) { } //Trace.WriteLine(toAddress); }
            string smtpFrom = "roboworksautomation@gmail.com";
            string smtpHost = "smtp.gmail.com";
            int smtpPort = 465; // Use 465 for SSL, 587 for TLS
            string smtpLogin = "roboworksautomation@gmail.com";
            string smtpPassword = "pzhtkrvxjwnryisg";
            string subject = "EnergyMeter Report";
            string content = "Hi All," +
                "Please find the attached report for yesterday" +
                "Thanks & Regards," +
                "Reports Team";
            //string attachmentPath = $@"{fileName}";

            var emailMessage = new MimeMessage();
            // Trace.WriteLine("here");
            emailMessage.From.Add(new MailboxAddress("RoboWorks", smtpFrom));
            // Trace.WriteLine(emailMessage.From);
            emailMessage.To.AddRange(toAddresses.Select(address => new MailboxAddress(address, address)));
            // Trace.WriteLine(emailMessage.To);
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = content };

            var multipart = new MimeKit.Multipart("mixed") { emailMessage.Body };

            if (File.Exists(fileName))
            {
                var attachment = new MimePart("application", "octet-stream")
                {
                    Content = new MimeContent(File.OpenRead(fileName)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    FileName = Path.GetFileName(fileName)
                };

                multipart.Add(attachment);
            }

            if (File.Exists(fileName1))
            {
                var attachment1 = new MimePart("application", "octet-stream")
                {
                    Content = new MimeContent(File.OpenRead(fileName1)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    FileName = Path.GetFileName(fileName1)
                };

                multipart.Add(attachment1);
            }
            emailMessage.Body = multipart;
            using (var client = new SmtpClient())
            {
                // Trace.WriteLine(fileName);
                try
                {
                    client.Connect(smtpHost, smtpPort, true); // Use TLS
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(smtpLogin, smtpPassword);
                    client.Send(emailMessage);

                    //  Trace.WriteLine("Email sent successfully.");
                }
                catch (Exception ex)
                {
                    // Trace.WriteLine($"Error sending email: {ex.Message}");
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }


        public async Task Sendmail()
        {
            // Start the email sending task independently
            var emailTask = SendEmailWithRetryAsync();

            // Start the download and delete tasks independently
            var reportTasks = Task.WhenAll(
                Task.Run(() => DownloadReportForPreviousDay1()),
                Task.Run(() => DeleteOldEntries(DateTime.Now.AddDays(-1)))
            );

            // Wait for both tasks to complete (if you want to wait for both email and report tasks)
            await Task.WhenAll(emailTask, reportTasks);

            // If you don't want to wait for email, remove the line below:
            // await emailTask;
        }
        public async Task SendEmailWithRetryAsync()
        {
            int maxRetries = 5;
            int delayInSeconds = 10;
            bool emailSent = false;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (IsInternetAvailable())
                    {
                        await SendDailyEmail3();  // Call your email sending logic here
                        emailSent = true;
                        break; // Exit loop if email is sent successfully
                    }
                    else
                    {
                        // Trace.WriteLine("Internet is not available. Retrying...");
                        await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));  // Wait for some time before retrying
                    }
                }
                catch (Exception ex)
                {
                    //Trace.WriteLine($"Error sending email: {ex.Message}");
                    LogError($"Email send attempt {i + 1} failed: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));  // Wait before retrying
                }
            }

            if (!emailSent)
            {
                //  Trace.WriteLine("Failed to send email after multiple attempts.");
                LogError("Failed to send email after multiple attempts.");
            }
        }

        private bool IsInternetAvailable()
        {
            try
            {
                // Check if there is an active internet connection by attempting to ping an external server
                using (var client = new WebClient())
                {
                    client.DownloadString("http://www.google.com");
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        private uint previousExecutionState;
        public void PreventSleepMode()
        {
            try
            {
                previousExecutionState = NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED | NativeMethods.ES_AWAYMODE_REQUIRED);
                if (previousExecutionState == 0)
                {
                    LogError("Failed to prevent sleep mode.");
                }
            }
            catch (Exception ex)
            {
                LogError($"PreventSleepMode error: {ex.Message}");
            }
        }

        public void AllowSleepMode()
        {
            try
            {
                if (previousExecutionState != 0)
                {
                    uint result = NativeMethods.SetThreadExecutionState(previousExecutionState);

                    if (result == 0)
                    {
                        LogError("Failed to restore sleep mode settings.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"AllowSleepMode error: {ex.Message}");
            }
        }

        public void LogError(string message)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                File.AppendAllText(logPath, $"{DateTime.Now}: {message}\n");

            }
            catch
            {
                // Avoid crashing the app if logging fails
            }
        }
    }

    // Native methods to prevent system sleep
    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern uint SetThreadExecutionState(uint esFlags);

        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;
        public const uint ES_AWAYMODE_REQUIRED = 0x00000040; // Prevent sleep mode
    }

}

