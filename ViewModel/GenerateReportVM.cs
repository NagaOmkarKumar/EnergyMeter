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
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.UniversalAccessibility.Drawing;
using LiveChartsCore.SkiaSharpView;
using PdfSharp.Charting;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Markup;
using System.Drawing;
using LiveChartsCore.Drawing;
using SkiaSharp;
using System.Windows.Controls;
using System.Globalization;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore;
using LiveChartsCore.VisualElements;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


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
        public List<string> Classes1 { get; set; }
        public string SelectedClass1 { get; set; }
        public GenerateReportVM()
        {
            Classes = new List<string> { "MeterData", "MinMaxValues", "EnergyData" };
            Classes1 = new List<string> { "PDF", "CSV" };
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

        private int[] CalculateColumnWidths<T>(IEnumerable<T> data, string[] headers, XGraphics gfx, XFont headerFont, XFont bodyFont, Func<T, string[]> getValues)
        {
            int[] columnWidths = new int[headers.Length];
            for (int i = 0; i < headers.Length; i++)
            {
                // Initialize with header width
                XSize headerSize = gfx.MeasureString(headers[i], headerFont);
                columnWidths[i] = (int)headerSize.Width + 10; // Add padding

                // Find maximum data width for this column
                foreach (var item in data)
                {
                    string[] values = getValues(item);
                    if (i < values.Length)
                    {
                        XSize valueSize = gfx.MeasureString(values[i], bodyFont);
                        int valueWidth = (int)valueSize.Width + 10; // Add padding
                        if (valueWidth > columnWidths[i])
                        {
                            columnWidths[i] = valueWidth;
                        }
                    }
                }
            }
            return columnWidths;
        }

        private int GetXPosition(int[] columnWidths, int columnIndex)
        {
            int xPosition = 0;
            for (int i = 0; i < columnIndex; i++)
            {
                xPosition += columnWidths[i];
            }
            return xPosition;
        }
        private void DownloadReport()
        {
            string selectedClass1 = SelectedClass1;
            string selectedClass = SelectedClass;
            if (selectedClass1 == "CSV")
            {
                FileStream stream = null;
                StreamWriter writer = null;
                FileStream stream1 = null;
                StreamWriter writer1 = null;
                FileStream stream2 = null;
                StreamWriter writer2 = null;
                

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
                        { writer.WriteLine("METERNAME,DATE,TIME,VOLTAGE,CURRENT,PF,Energy,ITHD%"); }

                        if (success >= 0)
                        {
                            List<DateTime> queryDates = GetDatesBetween(FromDate.Value, ToDate.Value);

                            //query_return_values.Clear();
                            try
                            {
                                foreach (DateTime date in queryDates)
                                {
                                    qu = date.ToString("dd-MM-yyyy");
                                    var q = connection.Table<MeterData>().Where(v => v.Date.StartsWith(qu)).ToList();

                                    foreach (MeterData report in q)
                                    {
                                        if (report != null)
                                        {
                                            writer.WriteLine($"{report.MeterName},{'*' + report.Date},{'*' + report.Timestamp},{report.Voltage},{report.Current},{report.PF},{report.KWH},{report.KVA}");
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
                        { writer1.WriteLine("METERNAME,DATE,MAXVOLTAGE,TIME,MAXCURRENT,TIME,MAXPF,TIME,MAXITHD,TIME,MINVOLTAGE,TIME,MINCURRENT,TIME,MINPF,TIME,MINITHD,TIME,Energy"); }
                        if (success >= 0)
                        {
                            List<DateTime> queryDates = GetDatesBetween(FromDate.Value, ToDate.Value);
                            //query_return_values.Clear();

                            try
                            {
                                foreach (DateTime date in queryDates)
                                {
                                    qu = date.ToString("dd-MM-yyyy");
                                    var q = connection.Table<MinMaxValues>().Where(v => v.Date.StartsWith(qu)).ToList();

                                    foreach (MinMaxValues report in q)
                                    {
                                        if (report != null && report.MaxEnergy != 0)
                                        {
                                            writer1.WriteLine($"{report.MeterName},{report.Date},{report.MaxVoltage},{report.MaxVoltageTime},{report.MaxCurrent},{report.MaxCurrentTime},{report.MaxPF},{report.MaxPFTime},{report.MaxTHD},{report.MaxTHDTime},{report.MinVoltage},{report.MinVoltageTime},{report.MinCurrent},{report.MinCurrentTime},{report.MinPF},{report.MinPFTime},{report.MinTHD},{report.MinTHDTime},{report.MaxEnergy}");

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
            if (selectedClass1 == "PDF")
            {
                try
                {
                    if (selectedClass == "MeterData")
                    {
                        int success = Validate(FromDate.Value.ToString(), ToDate.Value.ToString());
                        string qu = "";
                        string qu1 = GetFileName();

                        SQLiteConnection connection = new SQLiteConnection(App.databasePath);
                        PdfDocument pdf = new PdfDocument();

                        string fileName1 = qu1 + "\\";
                        string filename3 = "MeterDataReport" + FromDate.Value.ToString("dd-MM-yy") + "-" + ToDate.Value.ToString("dd-MM-yy") + DateTime.Now.ToString("HH-mm");
                        string filename2 = ".pdf";
                        string fileName0 = fileName1 + filename3 + filename2;
                        pdf.Info.Title = fileName0;
                        List<string> tempImages = new List<string>();

                        if (success >= 0)
                        {
                            List<DateTime> queryDates = GetDatesBetween(FromDate.Value, ToDate.Value);
                            int leftMargin = 40;
                            int rightMargin = 40;
                            int topMargin = 40;
                            int bottomMargin = 40;
                            int firstPageFooterHeight = 60; // Footer height for the first page
                            int otherPagesFooterHeight = 15; // Footer height for other pages
                            int firstPageheaderHeight = 100;
                            int headerHeight = 100;
                            int otherPagesheaderHeight = 70;
                            int rowHeight = 20;
                            XFont headerFont1 = new XFont("Times New Roman", 15, XFontStyleEx.Bold);
                            XFont dateFont = new XFont("Arial", 10, XFontStyleEx.Bold);
                            XFont headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
                            XFont bodyFont = new XFont("Arial", 9);

                            //int[] columnWidths;
                            int totalTableWidth;

                            bool isFirstPage = true;
                            int pageNumber = 1;
                            PdfPage page = null;
                            XGraphics gfx = null;
                            int tableY = 0;


                            //query_return_values.Clear();
                            try
                            {
                                foreach (DateTime date in queryDates)
                                {
                                    qu = date.ToString("dd-MM-yyyy");
                                    var q = connection.Table<MeterData>().Where(v => v.Date.StartsWith(qu)).ToList();
                                    if (q.Count == 0) continue;
                                    page = pdf.AddPage();
                                    gfx = XGraphics.FromPdfPage(page);
                                    tableY = headerHeight + topMargin;

                                   // DrawHeader2(gfx, isFirstPage, leftMargin, rightMargin, topMargin, page.Width);
                                   // DrawFooter2(gfx, isFirstPage, leftMargin, rightMargin, page.Width, page.Height, pageNumber, isFirstPage ? firstPageFooterHeight : otherPagesFooterHeight);
                                    gfx.DrawString($"MeterData Report - {FromDate.Value.ToString("dd-MM-yyyy") + " to " + ToDate.Value.ToString("dd-MM-yyyy")}",
                                    headerFont1, XBrushes.Black, new XPoint(150, 115));

                                    string[] headers = { "METERNAME", "DATE", "TIME", "VOLTAGE", "CURRENT", "PF", "ENERGY", "ITHD%" };
                                    int[] columnWidths = CalculateColumnWidths(q, headers, gfx, headerFont, bodyFont, 
                        item => new string[] { 
                            item.MeterName, item.Date, item.Timestamp, 
                            item.Voltage.ToString(), item.Current.ToString(), 
                            item.PF.ToString(), item.KWH.ToString(), item.KVA.ToString() 
                        });

                                    // Draw headers
                                    for (int i = 0; i < headers.Length; i++)
                                    {
                                        gfx.DrawRectangle(XPens.Black, new XRect(leftMargin + GetXPosition(columnWidths, i), tableY - 5, columnWidths[i], rowHeight));
                                        gfx.DrawString(headers[i], headerFont, XBrushes.Black, new XPoint(leftMargin + GetXPosition(columnWidths, i) + 5, tableY + 10));
                                    }
                                    tableY += rowHeight;

                                    foreach (MeterData report in q)
                                    {
                                        if (report != null)
                                        {
                                            string[] values = { report.MeterName, report.Date, report.Timestamp, report.Voltage.ToString(), report.Current.ToString(), report.PF.ToString(), report.KWH.ToString(), report.KVA.ToString() };
                                            for (int i = 0; i < values.Length; i++)
                                            {
                                                gfx.DrawRectangle(XPens.Black, new XRect(leftMargin + GetXPosition(columnWidths, i), tableY, columnWidths[i], rowHeight));
                                                gfx.DrawString(values[i], bodyFont, XBrushes.Black, new XPoint(leftMargin + GetXPosition(columnWidths, i) + 5, tableY + 15));
                                            }
                                            tableY += rowHeight;

                                            if (tableY + rowHeight > page.Height - 50)
                                            {
                                                pageNumber++;
                                                isFirstPage = false;
                                                page = pdf.AddPage();
                                                gfx = XGraphics.FromPdfPage(page);
                                                tableY = 75;
                                                //DrawHeader2(gfx, false, leftMargin, rightMargin, topMargin, page.Width);
                                                //DrawFooter2(gfx, false, leftMargin, rightMargin, page.Width, page.Height, pageNumber, otherPagesFooterHeight);

                                            }
                                           
                                        }

                                    }
                                    if (tableY + 30 + 220 > page.Height - 50)  // 30 for spacing + 220 for graph height
                                    {
                                        // Not enough space on current page, create a new page for graphs
                                        pageNumber++;
                                        isFirstPage = false;
                                        page = pdf.AddPage();
                                        gfx = XGraphics.FromPdfPage(page);
                                        tableY = 75; // Reset position on new page

                                       // DrawHeader2(gfx, false, leftMargin, rightMargin, topMargin, page.Width);
                                       // DrawFooter2(gfx, false, leftMargin, rightMargin, page.Width, page.Height, pageNumber, otherPagesFooterHeight);
                                    }
                                    else
                                    {
                                        // Enough space on current page, add spacing
                                        tableY += 30;
                                    }
                                    // tableY =+ 15;
                                    string[] parameters = { "Voltage", "Current", "PF", "ITHD%", "Energy" };
                                    foreach (var parameter in parameters)
                                    {

                                        string tempFilePath = GenerateGraph(q, parameter);
                                        if (!string.IsNullOrEmpty(tempFilePath)) tempImages.Add(tempFilePath);

                                        // Check if we need new page for graph
                                        if (tableY + 220 > page.Height - 50)
                                        {
                                            page = pdf.AddPage();
                                            gfx = XGraphics.FromPdfPage(page);
                                            tableY = 75;
                                           // DrawHeader2(gfx, isFirstPage, leftMargin, rightMargin, topMargin, page.Width);
                                           // DrawFooter2(gfx, isFirstPage, leftMargin, rightMargin, page.Width, page.Height, pageNumber, isFirstPage ? firstPageFooterHeight : otherPagesFooterHeight);
                                            //tableY = margin;
                                            //tableY += 20;
                                        }
                                       
                                        XImage img = XImage.FromFile(tempFilePath);
                                        gfx.DrawImage(img, leftMargin, tableY, 500, 200);
                                        tableY += 220;

                                    }
                                    //}
                                }

                                pdf.Save(fileName0);
                                foreach (var imgPath in tempImages)
                                {
                                    if (File.Exists(imgPath)) File.Delete(imgPath);
                                }

                                //MessageBox.Show($"Reports saved to:\nCSV: {csvPath}\nPDF: {pdfPath}",
                                //               "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                MessageBox.Show($"Report saved to {fileName0}", "Report Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                // } 
                            }
                            catch (Exception ex)
                            {
                                //Trace.WriteLine(ex.Message);
                                MessageBox.Show(ex.Message, "Error writing PDF", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else if (selectedClass == "MinMaxValues")
                    {
                        int success = Validate(FromDate.Value.ToString(), ToDate.Value.ToString());
                        string qu = "";
                        string qu1 = GetFileName();
                        SQLiteConnection connection = new SQLiteConnection(App.databasePath);
                        PdfDocument pdf = new PdfDocument();
                        pdf.PageLayout = PdfPageLayout.SinglePage;

                        string fileName1 = qu1 + "\\";
                        string filename5 = "MinMaxReport - " + FromDate.Value.ToString("dd-MM-yy") + "-" + ToDate.Value.ToString("dd-MM-yy") + DateTime.Now.ToString("HH-mm");
                        string filename2 = ".pdf";
                        string fileName = fileName1 + filename5 + filename2;
                        pdf.Info.Title = fileName;
                        List<string> tempImages = new List<string>();
                        if (success >= 0)
                        {
                            List<DateTime> queryDates = GetDatesBetween(FromDate.Value, ToDate.Value);
                            //query_return_values.Clear();
                            string[] headers = { "METERNAME", "DATE", "MAXVOLTAGE", "TIME", "MAXCURRENT", "TIME", "MAXPF", "TIME", "MAXITHD", "TIME", "MINVOLTAGE", "TIME", "MINCURRENT", "TIME", "MINPF", "TIME", "MINITHD", "TIME", "Energy" };
                            int margin = 20;
                            int headerHeight = 80;
                            int footerHeight = 30;
                            int rowHeight = 20;
                            XFont headerFont1 = new XFont("Times New Roman", 15, XFontStyleEx.Bold);
                            XFont headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
                            XFont bodyFont = new XFont("Arial", 8);
                            XFont dateFont = new XFont("Arial", 10, XFontStyleEx.Bold);

                            int[] columnWidths;
                            int totalTableWidth;

                            bool isFirstPage = true;
                            int pageNumber = 1;
                            PdfPage currentPage = null;
                            XGraphics gfx = null;
                            int tableY = 0;
                            int yPos = 0;

                            try
                            {
                                foreach (DateTime date in queryDates)
                                {
                                    qu = date.ToString("dd-MM-yyyy");
                                    var q = connection.Table<MinMaxValues>().Where(v => v.Date.StartsWith(qu)).ToList();

                                    if (q.Count == 0) continue;
                                    using (var tempDoc = new PdfDocument())
                                    {
                                        PdfPage tempPage = tempDoc.AddPage();
                                        tempPage.Orientation = PdfSharp.PageOrientation.Landscape;
                                        XGraphics tempGfx = XGraphics.FromPdfPage(tempPage);
                                        columnWidths = CalculateColumnWidths(q, headers, tempGfx, headerFont, bodyFont,
                           item => new string[] {
                                item.MeterName, item.Date, item.MaxVoltage.ToString(), item.MaxVoltageTime,
                                item.MaxCurrent.ToString(), item.MaxCurrentTime, item.MaxPF.ToString(), item.MaxPFTime,
                                item.MaxTHD.ToString(), item.MaxTHDTime, item.MinVoltage.ToString(), item.MinVoltageTime,
                                item.MinCurrent.ToString(), item.MinCurrentTime, item.MinPF.ToString(), item.MinPFTime,
                                item.MinTHD.ToString(), item.MinTHDTime, item.MaxEnergy.ToString()
                           });
                                        totalTableWidth = margin * 2 + columnWidths.Sum();
                                    }
                                    int dateHeight = (int)dateFont.GetHeight() + 10;
                                    int requiredHeight = dateHeight + rowHeight + rowHeight;

                                    if (currentPage == null || (tableY + requiredHeight > currentPage.Height - footerHeight))
                                    {
                                        if (currentPage != null) pageNumber++;
                                        currentPage = pdf.AddPage();
                                        currentPage.Orientation = PdfSharp.PageOrientation.Landscape;
                                        currentPage.Width = totalTableWidth;
                                        gfx = XGraphics.FromPdfPage(currentPage);
                                        DrawHeader1(gfx, isFirstPage, margin, totalTableWidth);
                                        Trace.WriteLine(totalTableWidth);
                                        DrawFooter1(gfx, isFirstPage, margin, totalTableWidth, currentPage.Height, pageNumber);
                                        tableY = margin + (isFirstPage ? headerHeight : 40);
                                        isFirstPage = false;
                                    }
                                    gfx.DrawString($"MinMaxValues Report - {FromDate.Value.ToString("dd-MM-yyyy") + " to " + ToDate.Value.ToString("dd-MM-yyyy")}",
                                   headerFont1, XBrushes.WhiteSmoke, new XPoint(250, 65));
                                    yPos += 30;
                                    // Draw date
                                    gfx.DrawString(qu, dateFont, XBrushes.Black,
                                        new XRect(margin, tableY + 25, currentPage.Width - 2 * margin, dateHeight),
                                        XStringFormats.TopLeft);
                                    tableY += dateHeight + 20;

                                    // Draw table headers
                                    for (int i = 0; i < headers.Length; i++)
                                    {
                                        gfx.DrawRectangle(XPens.Black,
                                            new XRect(margin + GetXPosition(columnWidths, i), tableY, columnWidths[i], rowHeight));
                                        gfx.DrawString(headers[i], headerFont, XBrushes.Black,
                                            new XPoint(margin + GetXPosition(columnWidths, i) + 5, tableY + 15));
                                    }
                                    tableY += rowHeight;

                                    foreach (MinMaxValues report in q)
                                    {
                                        if (report != null && report.MaxEnergy != 0)
                                        {
                                            if (tableY + rowHeight > currentPage.Height - footerHeight)
                                            {
                                                pageNumber++;
                                                // Add new page
                                                currentPage = pdf.AddPage();
                                                currentPage.Orientation = PdfSharp.PageOrientation.Landscape;
                                                currentPage.Width = totalTableWidth;
                                                gfx = XGraphics.FromPdfPage(currentPage);
                                                DrawHeader1(gfx, false, margin, totalTableWidth);
                                                DrawFooter1(gfx, isFirstPage, margin, totalTableWidth, currentPage.Height, pageNumber);
                                                tableY = margin + 40;

                                                gfx.DrawString(qu, dateFont, XBrushes.Black,
                               new XRect(margin, tableY, currentPage.Width - 2 * margin, dateHeight),
                               XStringFormats.TopLeft);
                                                tableY += dateHeight;
                                            }

                                            string[] values = { report.MeterName, report.Date, report.MaxVoltage.ToString(), report.MaxVoltageTime, report.MaxCurrent.ToString(), report.MaxCurrentTime, report.MaxPF.ToString(), report.MaxPFTime, report.MaxTHD.ToString(), report.MaxTHDTime, report.MinVoltage.ToString(), report.MinVoltageTime, report.MinCurrent.ToString(), report.MinCurrentTime, report.MinPF.ToString(), report.MinPFTime, report.MinTHD.ToString(), report.MinTHDTime, report.MaxEnergy.ToString() };
                                            for (int i = 0; i < values.Length; i++)
                                            {
                                                gfx.DrawRectangle(XPens.Black, new XRect(margin + GetXPosition(columnWidths, i), tableY, columnWidths[i], rowHeight));
                                                gfx.DrawString(values[i], bodyFont, XBrushes.Black, new XPoint(margin + GetXPosition(columnWidths, i) + 5, tableY + 15));
                                            }
                                            tableY += rowHeight;
                                        }
                                    }
                                }
                                pdf.Save(fileName);
                                MessageBox.Show($"Report saved to {fileName}", "Report Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                // Trace.WriteLine(ex.Message);
                                MessageBox.Show(ex.Message, "Error writing PDF", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else if (selectedClass == "EnergyData")
                    {
                        int success = Validate(FromDate.Value.ToString(), ToDate.Value.ToString());
                        SQLiteConnection connection = new SQLiteConnection(App.databasePath);
                        string fileName1 = GetFileName() + "\\";
                        string filename5 = "EnergyDataReport - " + FromDate.Value.ToString("dd-MM-yy") + "_" + ToDate.Value.ToString("dd-MM-yy") + "_" + DateTime.Now.ToString("HH-mm");
                        string filename4 = ".pdf";
                        string fileName0 = fileName1 + filename5 + filename4;
                        if (success >= 0)
                        {
                            List<DateTime> queryDates = GetDatesBetween(FromDate.Value, ToDate.Value);
                            try
                            {
                                // Create a new PDF document
                                PdfDocument document = new PdfDocument();
                                PdfPage page = document.AddPage();
                                XGraphics gfx = XGraphics.FromPdfPage(page);
                                XFont titleFont = new XFont("Arial", 16, XFontStyleEx.Bold);
                                XFont headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
                                XFont normalFont = new XFont("Arial", 10);
                                XFont footerFont = new XFont("Arial", 8);

                                // Set initial position for drawing
                                int leftMargin = 50;
                                int topMargin = 50;
                                int rightMargin = 50;
                                int firstPageFooterHeight = 60; // Footer height for the first page
                                int otherPagesFooterHeight = 40; // Footer height for other pages
                                int firstPageHeaderHeight = 100;
                                int otherPagesHeaderHeight = 50;
                                int rowHeight = 20;
                                
                                bool isFirstPage = true;
                                int pageNumber = 1;

                                double usablePageHeight = page.Height - topMargin - (isFirstPage ? firstPageHeaderHeight : otherPagesHeaderHeight) - (isFirstPage ? firstPageFooterHeight : otherPagesFooterHeight);
                                double yPosition = topMargin + (isFirstPage ? firstPageHeaderHeight : otherPagesHeaderHeight);

                                //DrawHeader(gfx, isFirstPage, leftMargin, rightMargin, topMargin, page.Width);
                                //DrawFooter(gfx, isFirstPage, leftMargin, rightMargin, page.Width, page.Height, pageNumber, firstPageFooterHeight);
                               
                                // Add a title to the PDF
                                gfx.DrawString("Energy Data Report", titleFont, XBrushes.Black, new XPoint(220, 110));
                                

                                string[] headers = { "METERNAME", "DATE", "ENERGY" };
                                int[] columnWidths = new int[] { 150, 150, 150 };

                                foreach (DateTime date in queryDates)
                                {
                                    string qu = date.ToString("dd-MM-yyyy");                                   
                                    var energyDataList = new List<EnergyData>();
                                    // Check if we need a new page for the date header and table
                                    if (yPosition + rowHeight * 2 + 15 > page.Height - (isFirstPage ? firstPageFooterHeight : otherPagesFooterHeight))
                                    {
                                        // Add a new page
                                        page = document.AddPage();
                                        gfx = XGraphics.FromPdfPage(page);
                                        pageNumber++;
                                        isFirstPage = false;

                                        // Reset position for new page
                                        yPosition = topMargin + otherPagesHeaderHeight;

                                        // Draw header and footer on new page
                                       // DrawHeader(gfx, isFirstPage, leftMargin, rightMargin, topMargin, page.Width);
                                       // DrawFooter(gfx, isFirstPage, leftMargin, rightMargin, page.Width, page.Height, pageNumber, otherPagesFooterHeight);
                                    }

                                    // Draw date header
                                    gfx.DrawString($"Date: {qu}", headerFont, XBrushes.Black, new XPoint(leftMargin, yPosition));
                                    yPosition += 5;

                                    // Draw table headers
                                    for (int i = 0; i < headers.Length; i++)
                                    {
                                        gfx.DrawRectangle(XPens.Black, new XRect(leftMargin + GetXPosition(columnWidths, i), yPosition, columnWidths[i], rowHeight));
                                        gfx.DrawString(headers[i], headerFont, XBrushes.Black, new XPoint(leftMargin + GetXPosition(columnWidths, i) + 5, yPosition + 15));
                                    }
                                    yPosition += rowHeight;                                  

                                    try
                                    {
                                        // Fetch data from the database (same as your existing code)
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
                                            .Select(d => d.MaxEnergy)
                                            .FirstOrDefault();

                                        double Welding1KWh = weldkwh.HasValue ? (weldkwh.Value - (compresskwh ?? 0)) : 1;
                                        double Welding2KWh = weld2kwh.HasValue ? (weld2kwh.Value - ((bracekwh ?? 0) + (robotkwh ?? 0))) : 1;
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

                                        foreach (var energyData in energyDataList)
                                        {
                                            if (energyData.KWH != 0)
                                            {
                                                double footerTopPosition = page.Height - (isFirstPage ? firstPageFooterHeight : otherPagesFooterHeight);

                                                // If this row would go into the footer area, create a new page
                                                if (yPosition + rowHeight > footerTopPosition)
                                                {
                                                    // Add a new page
                                                    page = document.AddPage();
                                                    gfx = XGraphics.FromPdfPage(page);
                                                    pageNumber++;
                                                    isFirstPage = false;

                                                    // Reset position for new page
                                                    yPosition = topMargin + otherPagesHeaderHeight;

                                                    // Draw header and footer on new page
                                                    //DrawHeader(gfx, isFirstPage, leftMargin, rightMargin, topMargin, page.Width);
                                                   // DrawFooter(gfx, isFirstPage, leftMargin, rightMargin, page.Width, page.Height, pageNumber, otherPagesFooterHeight);

                                                    // Draw table headers on the new page
                                                    for (int i = 0; i < headers.Length; i++)
                                                    {
                                                        gfx.DrawRectangle(XPens.Black, new XRect(leftMargin + GetXPosition(columnWidths, i), yPosition, columnWidths[i], rowHeight));
                                                        gfx.DrawString(headers[i], headerFont, XBrushes.Black, new XPoint(leftMargin + GetXPosition(columnWidths, i) + 5, yPosition + 15));
                                                    }
                                                    yPosition += rowHeight;
                                                }

                                                // Draw the data row
                                                for (int i = 0; i < headers.Length; i++)
                                                {
                                                    string cellValue = i == 0 ? energyData.MeterName : (i == 1 ? energyData.Date : energyData.KWH.ToString());
                                                    gfx.DrawRectangle(XPens.Black, new XRect(leftMargin + GetXPosition(columnWidths, i), yPosition, columnWidths[i], rowHeight));
                                                    gfx.DrawString(cellValue, normalFont, XBrushes.Black, new XPoint(leftMargin + GetXPosition(columnWidths, i) + 5, yPosition + 15));
                                                }
                                                yPosition += rowHeight;
                                            }
                                        }

                                        // Add space after each date's data
                                        yPosition += rowHeight + 15;
                                    }
                                    catch (Exception ex)
                                    {
                                        // Handle exceptions
                                        MessageBox.Show($"Error processing data for {qu}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }

                                // Save the PDF document
                                document.Save(fileName0);

                                MessageBox.Show($"Report saved to {fileName0}", "Report Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Error generating PDF", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }

                catch { }

                finally
                {

                }
            }
        }
        private void DrawHeader(XGraphics gfx, bool isFirstPage, int leftMargin, int rightMargin, int topMargin, double pageWidth)
        {
            XFont headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
            XFont companyFont = new XFont("Courier New", 14, XFontStyleEx.Bold);
            XFont companyFont1 = new XFont("Courier New", 10, XFontStyleEx.Regular);
            int headerY = 0;

            if (isFirstPage)
            {
                string imagePath = "D:\\WPF_Projects\\Project K\\Project K - JBM - WIP\\View\\assets\\LetterHead_DiffImage.png"; // Replace with actual path

                XImage logo = XImage.FromFile(imagePath);
                int logoHeight = 100;
                //gfx.DrawImage(logo, leftMargin, headerY, pageWidth - leftMargin - rightMargin, logoHeight);
                gfx.DrawImage(logo, 0, headerY, pageWidth, logoHeight);
                headerY += logoHeight + 18;
                gfx.DrawLine(XPens.Blue, leftMargin, headerY, pageWidth - rightMargin, headerY);
            }
            else
            {
                string imagePath = "D:\\WPF_Projects\\Project K\\Project K - JBM - WIP\\View\\assets\\RW_TextLOGO.jpg"; // Replace with actual path
                XImage logo = XImage.FromFile(imagePath);

                int logoHeight = 40;

                // Draw the logo on the left side
                //gfx.DrawImage(logo, leftMargin + 330, headerY, 250, 30);
                gfx.DrawImage(logo, 330, 10, 250, 30);
                headerY += 20;
                gfx.DrawLine(XPens.Blue, leftMargin, 50, pageWidth - rightMargin, 50);

            }
            
        }

        private void DrawFooter(XGraphics gfx, bool isFirstPage, int leftMargin, int rightMargin, double pageWidth, double pageHeight, int pageNumber, int footerHeight)
        {
            XFont footerFont = new XFont("Arial", 8, XFontStyleEx.Bold);
            int footerY = (int)pageHeight - 50;
            int footerY1 = (int)pageHeight - 40;
            int footerY2 = (int)pageHeight - 30;
            int footerY3 = (int)pageHeight - 20;
            
            if (isFirstPage)
            {
                gfx.DrawLine(XPens.Black, leftMargin, footerY, pageWidth - rightMargin, footerY);
                string imagePath = "D:\\WPF_Projects\\Project K\\Project K - JBM - WIP\\View\\assets\\Footer_Data.png";
                XImage logo = XImage.FromFile(imagePath);

                int logoHeight = 40;
                gfx.DrawImage(logo, 0, footerY1, pageWidth, logoHeight);
                //gfx.DrawImage(logo, leftMargin, footerY, pageWidth - leftMargin - rightMargin, logoHeight);
                footerY += logoHeight;
            }
            else
            {
                gfx.DrawLine(XPens.Blue, leftMargin, footerY1, pageWidth - rightMargin, footerY1);
                string imagePath = "D:\\WPF_Projects\\Project K\\Project K - JBM - WIP\\View\\assets\\Footer.png"; // Replace with actual path
                XImage logo = XImage.FromFile(imagePath);                
                int logoHeight =30;
                gfx.DrawImage(logo, leftMargin, footerY2, pageWidth- rightMargin-leftMargin, logoHeight);
                // gfx.DrawImage(logo, leftMargin, footerY, pageWidth - leftMargin - rightMargin, logoHeight);
                // Draw the logo on the left side
                //string companyName1 = pageNumber.ToString();
                XRect footerRect = new XRect(0, footerY2, pageWidth - rightMargin, logoHeight);
                gfx.DrawString($"Page {pageNumber}", footerFont, XBrushes.White,
                    footerRect, XStringFormats.Center);
                //gfx.DrawString($"Page {pageNumber}", footerFont, XBrushes.White,
                //    new XRect(pageWidth - 300, 10, 80, 30), XStringFormats.BottomLeft);
                footerY += logoHeight;
            }
        }

        private void DrawHeader2(XGraphics gfx, bool isFirstPage, int leftMargin, int rightMargin, int topMargin, double pageWidth)
        {
            XFont headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
            XFont companyFont = new XFont("Courier New", 14, XFontStyleEx.Bold);
            XFont companyFont1 = new XFont("Courier New", 10, XFontStyleEx.Regular);
            int headerY = 0;

            if (isFirstPage)
            {
                string imagePath = "D:\\WPF_Projects\\Project K\\Project K - JBM - WIP\\View\\assets\\LetterHead_Color.png"; // Replace with actual path

                XImage logo = XImage.FromFile(imagePath);
                int logoHeight = 100;
                //gfx.DrawImage(logo, leftMargin, headerY, pageWidth - leftMargin - rightMargin, logoHeight);
                gfx.DrawImage(logo, 0, headerY, pageWidth, logoHeight);
                headerY += logoHeight + 18;
                gfx.DrawLine(XPens.Black, leftMargin, headerY, pageWidth - rightMargin, headerY);
            }
            else
            {
                string imagePath = "D:\\WPF_Projects\\Project K\\Project K - JBM - WIP\\View\\assets\\RW_TextLOGO.jpg"; // Replace with actual path
                XImage logo = XImage.FromFile(imagePath);

                int logoHeight = 40;

                // Draw the logo on the left side
               //gfx.DrawImage(logo, leftMargin + 330, headerY, 250, 30);
                gfx.DrawImage(logo, 330, 10, 250, 30);
                headerY += 20;
                gfx.DrawLine(XPens.Black, leftMargin, 50, pageWidth - rightMargin, 50);

            }

        }

        private void DrawFooter2(XGraphics gfx, bool isFirstPage, int leftMargin, int rightMargin, double pageWidth, double pageHeight, int pageNumber, int footerHeight)
        {
            XFont footerFont = new XFont("Arial", 8, XFontStyleEx.Bold);
            int footerY = (int)pageHeight - 50;
            int footerY1 = (int)pageHeight - 40;
            int footerY2 = (int)pageHeight - 30;
            int footerY3 = (int)pageHeight - 20;

            if (isFirstPage)
            {
                gfx.DrawLine(XPens.Black, leftMargin, footerY, pageWidth - rightMargin, footerY);
                string imagePath = "D:\\WPF_Projects\\Project K\\Project K - JBM - WIP\\View\\assets\\Footer_Data.png";
                XImage logo = XImage.FromFile(imagePath);

                int logoHeight = 40;
                gfx.DrawImage(logo, 0, footerY1, pageWidth, logoHeight);
                //gfx.DrawImage(logo, leftMargin, footerY, pageWidth - leftMargin - rightMargin, logoHeight);
                footerY += logoHeight;
            }
            else
            {
                gfx.DrawLine(XPens.Black, leftMargin, footerY1, pageWidth - rightMargin, footerY1);
                string imagePath = "D:\\WPF_Projects\\Project K\\Project K - JBM - WIP\\View\\assets\\Footer.png"; // Replace with actual path
                XImage logo = XImage.FromFile(imagePath);
                int logoHeight = 30;
                gfx.DrawImage(logo, leftMargin, footerY2, pageWidth - rightMargin - leftMargin, logoHeight);
                 //gfx.DrawImage(logo, leftMargin, footerY, pageWidth - leftMargin - rightMargin, logoHeight);
                // Draw the logo on the left side
                //string companyName1 = pageNumber.ToString();
                XRect footerRect = new XRect(0, footerY2, pageWidth - rightMargin, logoHeight);
                gfx.DrawString($"Page {pageNumber}", footerFont, XBrushes.White,
                    footerRect, XStringFormats.Center);
                //gfx.DrawString($"Page {pageNumber}", footerFont, XBrushes.White,
                    //new XRect(pageWidth - 300, 10, 80, 30), XStringFormats.BottomLeft);
                footerY += logoHeight;
            }
        }
        private void DrawHeader1(XGraphics gfx, bool isFirstPage, int margin, double pageWidth)
        {
            XFont headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
            XFont companyFont = new XFont("Courier New", 14, XFontStyleEx.Bold);
            XFont companyFont1 = new XFont("Courier New", 10, XFontStyleEx.Regular);
            int headerY = 0;

            if (isFirstPage)
            {
                // Load the image (adjust path as needed)
                string imagePath = "D:\\WPF_Projects\\Project K\\Project K - JBM - WIP\\View\\assets\\Darkinside.png"; // Replace with actual path
                XImage logo = XImage.FromFile(imagePath);

                // Define image dimensions
                //int logoWidth = tablewidth + 40;  // Adjust based on your logo size
                int logoHeight = 100; 

                // Draw the logo on the left side
                gfx.DrawImage(logo, 0, headerY, pageWidth, logoHeight);

                headerY += logoHeight + 10;
            }
            else
            {
                string imagePath = "D:\\WPF_Projects\\Project K\\Project K - JBM - WIP\\View\\assets\\RW_TextLOGO.jpg"; // Replace with actual path
                XImage logo = XImage.FromFile(imagePath);

                // Define image dimensions
                //int logoWidth = tablewidth + 40;  // Adjust based on your logo size
                //int logoHeight = 40;

                // Draw the logo on the left side
                gfx.DrawImage(logo, 700, 10, 300, 40);
                headerY += 20;
            }

        }

        private void DrawFooter1(XGraphics gfx, bool isFirstPage, int margin, double pageWidth, double pageHeight, int pageNumber)
        {
            XFont footerFont = new XFont("Arial", 8);
            int footerY = (int)pageHeight - 50;

            if (isFirstPage)
            {
                string imagePath = "D:\\WPF_Projects\\Project K\\Project K - JBM - WIP\\View\\assets\\FooterAddress.png"; // Replace with actual path
                XImage logo = XImage.FromFile(imagePath);

                // Define image dimensions
                //int logoWidth = tablewidth + 40;  // Adjust based on your logo size
                int logoHeight =50;

                // Draw the logo on the left side
                gfx.DrawImage(logo, 0, footerY, pageWidth, logoHeight);

                footerY += logoHeight;
            }
            else
            {
                string imagePath = "D:\\WPF_Projects\\Project K\\Project K - JBM - WIP\\View\\assets\\Footer.png"; // Replace with actual path
                XImage logo = XImage.FromFile(imagePath);

                // Define image dimensions
                //int logoWidth = tablewidth + 40;  // Adjust based on your logo size
                int logoHeight = 40;

                // Draw the logo on the left side
                gfx.DrawImage(logo, 0, footerY, pageWidth , logoHeight);

                // Draw the company name on the right side
                //string companyName = "ROBOWORKS AUTOMATION";
                //gfx.DrawString(companyName, companyFont, XBrushes.Black,
                //    new XRect(pageWidth - margin - 150, headerY + 15, 150, 20), XStringFormats.TopRight);
                string companyName1 = pageNumber.ToString();
                gfx.DrawString($"Page {pageNumber}", footerFont, XBrushes.Black,
                    new XRect(pageWidth - margin - 50, footerY + 10, 150, 20), XStringFormats.BottomCenter);

                footerY += logoHeight;
            }
        }
        private string GenerateGraph(IEnumerable<MeterData> data, string parameter)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"{parameter}_{Guid.NewGuid()}.png");
            var orderedData = data.OrderBy(d => d.Timestamp).ToList();

            var chart = new SKCartesianChart
            {
                Width = 900, // Increased for better resolution
                Height = 400,
                Series = new ISeries[]
                {
            new LineSeries<double>
            {
                Name = parameter,
                Values = orderedData.Select(d => GetParameterValue(d, parameter)),
                GeometrySize = 0,
                Stroke = new SolidColorPaint(SKColors.Blue, 3),
                Fill = null
            }
                },
                XAxes = new List<LiveChartsCore.SkiaSharpView.Axis>
        {
            new LiveChartsCore.SkiaSharpView.Axis
            {
                Labels = orderedData.Select(d => d.Timestamp).ToArray(),
                LabelsRotation = 45,
                TextSize = 12,
                Name = "Time",
                NamePaint = new SolidColorPaint(SKColors.Black),
                LabelsPaint = new SolidColorPaint(SKColors.Black)
            }
        },
                YAxes = new List<LiveChartsCore.SkiaSharpView.Axis>
        {
            new LiveChartsCore.SkiaSharpView.Axis
            {
                Name = parameter,
                TextSize = 12,
                NamePaint = new SolidColorPaint(SKColors.Black),
                LabelsPaint = new SolidColorPaint(SKColors.Black)
            }
        },
                Title = new LiveChartsCore.SkiaSharpView.VisualElements.LabelVisual
                {
                    Text = $"{parameter} Trend",
                    TextSize = 18,
                    Padding = new Padding(15),
                    Paint = new SolidColorPaint(SKColors.Black)
                },
                Background = SKColors.White
            };

            try
            {
                chart.SaveImage(tempFilePath);
                Debug.WriteLine($"Chart saved to: {tempFilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving chart: {ex}");
                return null;
            }

            return tempFilePath;
        }

        // Helper function to extract the parameter value
        private double GetParameterValue(MeterData data, string parameter)
        {
            return parameter switch
            {
                "Voltage" => data.Voltage,
                "Current" => data.Current,
                "PF" => data.PF,
                "ITHD%" => data.KVA,
                "Energy" => data.KWH,
                _ => 0
            };
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
            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                // === Generate CSV Report ===
                try
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
                        File.WriteAllText(fileName, csvContent.ToString(), Encoding.UTF8);
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("CSV Generation Failed: " + ex.Message, "CSV Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                int success = Validate(FromDate.Value.ToString(), ToDate.Value.ToString());

                string fileName2 = GetFileName() + "\\";
                string filename5 = "EnergyDataReport - " + FromDate.Value.ToString("dd-MM-yy") + "_" + ToDate.Value.ToString("dd-MM-yy") + "_" + DateTime.Now.ToString("HH-mm");
                string filename6 = ".pdf";
                string fileName0 = fileName2 + filename5 + filename6;
                if (success >= 0)
                {
                    List<DateTime> queryDates = GetDatesBetween(FromDate.Value, ToDate.Value);
                    try
                    {
                        // Create a new PDF document
                        PdfDocument document = new PdfDocument();
                        PdfPage page = document.AddPage();
                        XGraphics gfx = XGraphics.FromPdfPage(page);
                        XFont titleFont = new XFont("Arial", 16, XFontStyleEx.Bold);
                        XFont headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
                        XFont normalFont = new XFont("Arial", 10);
                        XFont footerFont = new XFont("Arial", 8);

                        // Set initial position for drawing
                        int leftMargin = 50;
                        int topMargin = 50;
                        int rightMargin = 50;
                        int firstPageFooterHeight = 60; // Footer height for the first page
                        int otherPagesFooterHeight = 40; // Footer height for other pages
                        int firstPageHeaderHeight = 100;
                        int otherPagesHeaderHeight = 50;
                        int rowHeight = 20;

                        bool isFirstPage = true;
                        int pageNumber = 1;

                        double usablePageHeight = page.Height - topMargin - (isFirstPage ? firstPageHeaderHeight : otherPagesHeaderHeight) - (isFirstPage ? firstPageFooterHeight : otherPagesFooterHeight);
                        double yPosition = topMargin + (isFirstPage ? firstPageHeaderHeight : otherPagesHeaderHeight);

                        //DrawHeader(gfx, isFirstPage, leftMargin, rightMargin, topMargin, page.Width);
                        //DrawFooter(gfx, isFirstPage, leftMargin, rightMargin, page.Width, page.Height, pageNumber, firstPageFooterHeight);

                        // Add a title to the PDF
                        gfx.DrawString("Energy Data Report", titleFont, XBrushes.Black, new XPoint(220, 110));


                        string[] headers = { "METERNAME", "DATE", "ENERGY" };
                        int[] columnWidths = new int[] { 150, 150, 150 };

                        foreach (DateTime date in queryDates)
                        {
                            string qu = date.ToString("dd-MM-yyyy");
                            var energyDataList = new List<EnergyData>();
                            // Check if we need a new page for the date header and table
                            if (yPosition + rowHeight * 2 + 15 > page.Height - (isFirstPage ? firstPageFooterHeight : otherPagesFooterHeight))
                            {
                                // Add a new page
                                page = document.AddPage();
                                gfx = XGraphics.FromPdfPage(page);
                                pageNumber++;
                                isFirstPage = false;

                                // Reset position for new page
                                yPosition = topMargin + otherPagesHeaderHeight;

                                // Draw header and footer on new page
                                // DrawHeader(gfx, isFirstPage, leftMargin, rightMargin, topMargin, page.Width);
                                // DrawFooter(gfx, isFirstPage, leftMargin, rightMargin, page.Width, page.Height, pageNumber, otherPagesFooterHeight);
                            }

                            // Draw date header
                            gfx.DrawString($"Date: {qu}", headerFont, XBrushes.Black, new XPoint(leftMargin, yPosition));
                            yPosition += 5;

                            // Draw table headers
                            for (int i = 0; i < headers.Length; i++)
                            {
                                gfx.DrawRectangle(XPens.Black, new XRect(leftMargin + GetXPosition(columnWidths, i), yPosition, columnWidths[i], rowHeight));
                                gfx.DrawString(headers[i], headerFont, XBrushes.Black, new XPoint(leftMargin + GetXPosition(columnWidths, i) + 5, yPosition + 15));
                            }
                            yPosition += rowHeight;

                            try
                            {
                                // Fetch data from the database (same as your existing code)
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
                                    .Select(d => d.MaxEnergy)
                                    .FirstOrDefault();

                                double Welding1KWh = weldkwh.HasValue ? (weldkwh.Value - (compresskwh ?? 0)) : 1;
                                double Welding2KWh = weld2kwh.HasValue ? (weld2kwh.Value - ((bracekwh ?? 0) + (robotkwh ?? 0))) : 1;
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

                                foreach (var energyData in energyDataList)
                                {
                                    if (energyData.KWH != 0)
                                    {
                                        double footerTopPosition = page.Height - (isFirstPage ? firstPageFooterHeight : otherPagesFooterHeight);

                                        // If this row would go into the footer area, create a new page
                                        if (yPosition + rowHeight > footerTopPosition)
                                        {
                                            // Add a new page
                                            page = document.AddPage();
                                            gfx = XGraphics.FromPdfPage(page);
                                            pageNumber++;
                                            isFirstPage = false;

                                            // Reset position for new page
                                            yPosition = topMargin + otherPagesHeaderHeight;

                                            // Draw header and footer on new page
                                            //DrawHeader(gfx, isFirstPage, leftMargin, rightMargin, topMargin, page.Width);
                                            // DrawFooter(gfx, isFirstPage, leftMargin, rightMargin, page.Width, page.Height, pageNumber, otherPagesFooterHeight);

                                            // Draw table headers on the new page
                                            for (int i = 0; i < headers.Length; i++)
                                            {
                                                gfx.DrawRectangle(XPens.Black, new XRect(leftMargin + GetXPosition(columnWidths, i), yPosition, columnWidths[i], rowHeight));
                                                gfx.DrawString(headers[i], headerFont, XBrushes.Black, new XPoint(leftMargin + GetXPosition(columnWidths, i) + 5, yPosition + 15));
                                            }
                                            yPosition += rowHeight;
                                        }

                                        // Draw the data row
                                        for (int i = 0; i < headers.Length; i++)
                                        {
                                            string cellValue = i == 0 ? energyData.MeterName : (i == 1 ? energyData.Date : energyData.KWH.ToString());
                                            gfx.DrawRectangle(XPens.Black, new XRect(leftMargin + GetXPosition(columnWidths, i), yPosition, columnWidths[i], rowHeight));
                                            gfx.DrawString(cellValue, normalFont, XBrushes.Black, new XPoint(leftMargin + GetXPosition(columnWidths, i) + 5, yPosition + 15));
                                        }
                                        yPosition += rowHeight;
                                    }
                                }

                                // Add space after each date's data
                                yPosition += rowHeight + 15;
                            }
                            catch (Exception ex)
                            {
                                // Handle exceptions
                               // MessageBox.Show($"Error processing data for {qu}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }

                            // Save the PDF document
                            document.Save(fileName0);

                            //MessageBox.Show($"Report saved to {fileName0}", "Report Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        //Trace.WriteLine("Error writing CSV: " + ex.Message);
                        // MessageBox.Show(ex.Message, "Error writing CSV", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
                

        private void DownloadMinmaxReportForPreviousDay()
        {
            SQLiteConnection connection = new SQLiteConnection(App.databasePath);
            try
            {
                string fileName1 = GetFileName() + "\\";
            string filename3 = "MinMax_Report_" + DateTime.Now.AddDays(-1).ToString("dd-MM-yy");
            string filename4 = ".csv";
            string fileNamee = fileName1 + filename3 + filename4;


            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("METERNAME,DATE,MAXVOLTAGE,TIME,MAXCURRENT,TIME,MAXPF,TIME,MINVOLTAGE,TIME,MINCURRENT,TIME,MINPF,TIME,MAXTHD,TIME,MINTHD,TIME,ENERGY");

            //using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                string queryDate = DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy");
                var query = connection.Table<MinMaxValues>().Where(v => v.Date.StartsWith(queryDate));
                if (query != null)
                {
                    foreach (MinMaxValues report in query)
                    {
                        if (report != null && report.MaxEnergy != 0)
                        {
                            csvContent.AppendLine($"{report.MeterName},{'*' + report.Date},{report.MaxVoltage},{'*' + report.MaxVoltageTime},{report.MaxCurrent},{'*' + report.MaxCurrentTime},{report.MaxPF},{'*' + report.MaxPFTime},{report.MinVoltage},{'*' + report.MinVoltageTime},{report.MinCurrent},{'*' + report.MinCurrentTime},{report.MinPF},{'*' + report.MinPFTime},{report.MaxTHD},{report.MaxTHDTime},{report.MinTHD},{report.MinTHDTime},{report.MaxEnergy}");
                        }
                    }
                }
            }
           
                File.WriteAllText(fileNamee, csvContent.ToString(), Encoding.UTF8);
                // Trace.WriteLine("Downloaded");
            }
            catch (Exception ex)
            {
               // MessageBox.Show($"Error saving CSV: {ex.Message}", "CSV Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            DateTime previousDate = DateTime.Now.AddDays(-1);
            int success = Validate(previousDate.ToString("dd-MM-yyyy"), previousDate.ToString("dd-MM-yyyy"));
            string qu = "";
            string qu1 = GetFileName();
            string queryDateStr = previousDate.ToString("dd-MM-yyyy");
            PdfDocument pdf = new PdfDocument();
            pdf.PageLayout = PdfPageLayout.SinglePage;

            string fileName0 = qu1 + "\\";
            string filename6 = "MinMaxReport - " + DateTime.Now.AddDays(-1).ToString("dd-MM-yy");
            string filename2 = ".pdf";
            string fileName = fileName0 + filename6 + filename2;
            pdf.Info.Title = fileName;
            List<string> tempImages = new List<string>();
            if (success >= 0)
            {
                // List<DateTime> queryDates = GetDatesBetween(previousDate.ToString("dd-MM-yyyy"), previousDate.ToString("dd-MM-yyyy"));
                {
                    //var query = connection.Table<MinMaxValues>().Where(v => v.Date.StartsWith(queryDateStr));
                    //query_return_values.Clear();
                    string[] headers = { "METERNAME", "DATE", "MAXVOLTAGE", "TIME", "MAXCURRENT", "TIME", "MAXPF", "TIME", "MAXITHD", "TIME", "MINVOLTAGE", "TIME", "MINCURRENT", "TIME", "MINPF", "TIME", "MINITHD", "TIME", "Energy" };
                    int margin = 20;
                    int headerHeight = 80;
                    int footerHeight = 30;
                    int rowHeight = 20;
                    XFont headerFont1 = new XFont("Times New Roman", 15, XFontStyleEx.Bold);
                    XFont headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
                    XFont bodyFont = new XFont("Arial", 8);
                    XFont dateFont = new XFont("Arial", 10, XFontStyleEx.Bold);

                    int[] columnWidths;
                    int totalTableWidth;

                    bool isFirstPage = true;
                    int pageNumber = 1;
                    PdfPage currentPage = null;
                    XGraphics gfx = null;
                    int tableY = 0;
                    int yPos = 0;

                    try
                    {
                        // foreach (DateTime date in queryDates)
                        // qu = date.ToString("dd-MM-yyyy");
                        var q = connection.Table<MinMaxValues>().Where(v => v.Date.StartsWith(queryDateStr)).ToList();

                        //if (q.Count == 0) continue;
                        using (var tempDoc = new PdfDocument())
                        {
                            PdfPage tempPage = tempDoc.AddPage();
                            tempPage.Orientation = PdfSharp.PageOrientation.Landscape;
                            XGraphics tempGfx = XGraphics.FromPdfPage(tempPage);
                            columnWidths = CalculateColumnWidths(q, headers, tempGfx, headerFont, bodyFont,
               item => new string[] {
                                item.MeterName, item.Date, item.MaxVoltage.ToString(), item.MaxVoltageTime,
                                item.MaxCurrent.ToString(), item.MaxCurrentTime, item.MaxPF.ToString(), item.MaxPFTime,
                                item.MaxTHD.ToString(), item.MaxTHDTime, item.MinVoltage.ToString(), item.MinVoltageTime,
                                item.MinCurrent.ToString(), item.MinCurrentTime, item.MinPF.ToString(), item.MinPFTime,
                                item.MinTHD.ToString(), item.MinTHDTime, item.MaxEnergy.ToString()
               });
                            totalTableWidth = margin * 2 + columnWidths.Sum();
                        }
                        int dateHeight = (int)dateFont.GetHeight() + 10;
                        int requiredHeight = dateHeight + rowHeight + rowHeight;

                        if (currentPage == null || (tableY + requiredHeight > currentPage.Height - footerHeight))
                        {
                            if (currentPage != null) pageNumber++;
                            currentPage = pdf.AddPage();
                            currentPage.Orientation = PdfSharp.PageOrientation.Landscape;
                            currentPage.Width = totalTableWidth;
                            gfx = XGraphics.FromPdfPage(currentPage);
                            DrawHeader1(gfx, isFirstPage, margin, totalTableWidth);
                            Trace.WriteLine(totalTableWidth);
                            DrawFooter1(gfx, isFirstPage, margin, totalTableWidth, currentPage.Height, pageNumber);
                            tableY = margin + (isFirstPage ? headerHeight : 40);
                            isFirstPage = false;
                        }
                        gfx.DrawString($"MinMaxValues Report - {FromDate.Value.ToString("dd-MM-yyyy") + " to " + ToDate.Value.ToString("dd-MM-yyyy")}",
                       headerFont1, XBrushes.WhiteSmoke, new XPoint(250, 65));
                        yPos += 30;
                        // Draw date
                        gfx.DrawString(qu, dateFont, XBrushes.Black,
                            new XRect(margin, tableY + 25, currentPage.Width - 2 * margin, dateHeight),
                            XStringFormats.TopLeft);
                        tableY += dateHeight + 20;

                        // Draw table headers
                        for (int i = 0; i < headers.Length; i++)
                        {
                            gfx.DrawRectangle(XPens.Black,
                                new XRect(margin + GetXPosition(columnWidths, i), tableY, columnWidths[i], rowHeight));
                            gfx.DrawString(headers[i], headerFont, XBrushes.Black,
                                new XPoint(margin + GetXPosition(columnWidths, i) + 5, tableY + 15));
                        }
                        tableY += rowHeight;

                        foreach (MinMaxValues report in q)
                        {
                            if (report != null && report.MaxEnergy != 0)
                            {
                                if (tableY + rowHeight > currentPage.Height - footerHeight)
                                {
                                    pageNumber++;
                                    // Add new page
                                    currentPage = pdf.AddPage();
                                    currentPage.Orientation = PdfSharp.PageOrientation.Landscape;
                                    currentPage.Width = totalTableWidth;
                                    gfx = XGraphics.FromPdfPage(currentPage);
                                    DrawHeader1(gfx, false, margin, totalTableWidth);
                                    DrawFooter1(gfx, isFirstPage, margin, totalTableWidth, currentPage.Height, pageNumber);
                                    tableY = margin + 40;

                                    gfx.DrawString(qu, dateFont, XBrushes.Black,
                   new XRect(margin, tableY, currentPage.Width - 2 * margin, dateHeight),
                   XStringFormats.TopLeft);
                                    tableY += dateHeight;
                                }

                                string[] values = { report.MeterName, report.Date, report.MaxVoltage.ToString(), report.MaxVoltageTime, report.MaxCurrent.ToString(), report.MaxCurrentTime, report.MaxPF.ToString(), report.MaxPFTime, report.MaxTHD.ToString(), report.MaxTHDTime, report.MinVoltage.ToString(), report.MinVoltageTime, report.MinCurrent.ToString(), report.MinCurrentTime, report.MinPF.ToString(), report.MinPFTime, report.MinTHD.ToString(), report.MinTHDTime, report.MaxEnergy.ToString() };
                                for (int i = 0; i < values.Length; i++)
                                {
                                    gfx.DrawRectangle(XPens.Black, new XRect(margin + GetXPosition(columnWidths, i), tableY, columnWidths[i], rowHeight));
                                    gfx.DrawString(values[i], bodyFont, XBrushes.Black, new XPoint(margin + GetXPosition(columnWidths, i) + 5, tableY + 15));
                                }
                                tableY += rowHeight;
                            }
                        }

                        pdf.Save(fileName);
                        //MessageBox.Show($"Report saved to {fileName}", "Report Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        // Trace.WriteLine(ex.Message);
                        // MessageBox.Show(ex.Message, "Error writing PDF", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
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
            string fileName2 = Path.Combine(GetFileName() + "\\", "EnergyDataReport - " + DateTime.Now.AddDays(-1).ToString("dd-MM-yy") + ".pdf");
            string fileName3 = Path.Combine(GetFileName() + "\\", "MinMaxReport -" + DateTime.Now.AddDays(-1).ToString("dd-MM-yy") + ".pdf");
            // Trace.WriteLine("Email");
            if (!File.Exists(fileName) || !File.Exists(fileName1) || !File.Exists(fileName3))
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
            if (File.Exists(fileName3))
            {
                var attachment3 = new MimePart("application", "octet-stream")
                {
                    Content = new MimeContent(File.OpenRead(fileName3)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    FileName = Path.GetFileName(fileName3)
                };

                multipart.Add(attachment3);
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

