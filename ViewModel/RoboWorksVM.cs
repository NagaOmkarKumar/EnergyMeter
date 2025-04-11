using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Project_K.Model;
using Project_K.View;
using Project_K.ViewModel.Helpers;
using SkiaSharp;
using SQLite;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using MailKit.Net.Smtp;
using MimeKit;
using LiveChartsCore.Defaults;
using System.Windows.Media.Effects;
using System.Diagnostics.Metrics;
using System.Globalization;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using System.Collections.ObjectModel;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics.Eventing.Reader;
using System.Text.RegularExpressions;

namespace Project_K.ViewModel
{
    public class RoboWorksVM : ObservableObject
    {
        public GenerateReportVM GenerateReportViewModel { get; set; }
        public static RoboWorksVM Instance { get; private set; }
        private System.Timers.Timer _pollingTimer;
        private List<string> _ipAddresses;
        private readonly HttpClient _httpClient = new HttpClient();
        public ICommand RefreshCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand MeterInfoCommand { get; }

        public List<Thread> threads = new List<Thread>();

        private ISeries[] _series;
        public ISeries[] Series
        {
            get => _series;
            set => SetProperty(ref _series, value);
        }

        private ISeries[] _series1;
        public ISeries[] Series1
        {
            get => _series1;
            set => SetProperty(ref _series1, value);
        }

        private ISeries[] _series2;
        public ISeries[] Series2
        {
            get => _series2;
            set => SetProperty(ref _series2, value);
        }

        private ISeries[] _series3;
        public ISeries[] Series3
        {
            get => _series3;
            set => SetProperty(ref _series3, value);
        }

        private LabelVisual _title;
        public LabelVisual Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private LabelVisual _title1;
        public LabelVisual Title1
        {
            get => _title1;
            set => SetProperty(ref _title1, value);
        }

        private LabelVisual _title2;
        public LabelVisual Title2
        {
            get => _title2;
            set => SetProperty(ref _title2, value);
        }
        private LabelVisual _title3;
        public LabelVisual Title3
        {
            get => _title3;
            set => SetProperty(ref _title3, value);
        }

        private Axis[] _xAxes;
        public Axis[] XAxes
        {
            get => _xAxes;
            set => SetProperty(ref _xAxes, value);
        }

        private string _selectedMeter;
        public string SelectedMeter
        {
            get => _selectedMeter;
            set
            {
                if (SetProperty(ref _selectedMeter, value))
                {
                    UpdateChartData();
                }
            }
        }

        private IEnumerable<string> _meters;
        public IEnumerable<string> Meters
        {
            get => _meters;
            set => SetProperty(ref _meters, value);
        }

        private string _maxPF;
        public string MaxPF
        {
            get => _maxPF;
            set => SetProperty(ref _maxPF, value);
        }

        private string _minPF;
        public string MinPF
        {
            get => _minPF;
            set => SetProperty(ref _minPF, value);
        }

        private string _maxVoltage;
        public string MaxVoltage
        {
            get => _maxVoltage;
            set => SetProperty(ref _maxVoltage, value);
        }

        private string _maxTHD;
        public string MaxTHD
        {
            get => _maxTHD;
            set => SetProperty(ref _maxTHD, value);
        }

        private string _minTHD;
        public string MinTHD
        {
            get => _minTHD;
            set => SetProperty(ref _minTHD, value);
        }

        private string _maxCurrent;
        public string MaxCurrent
        {
            get => _maxCurrent;
            set => SetProperty(ref _maxCurrent, value);
        }

        private string _minVoltage;
        public string MinVoltage
        {
            get => _minVoltage;
            set => SetProperty(ref _minVoltage, value);
        }

        private string _minCurrent;
        public string MinCurrent
        {
            get => _minCurrent;
            set => SetProperty(ref _minCurrent, value);
        }
      
        private string _today;
        public string Today
        {
            get => _today;
            set => SetProperty(ref _today, value);
        }

        public RoboWorksVM()
        {
            Series = new ISeries[]
            {
                new LineSeries<double>
                {   Name = "PowerFactor",
                    Values = new ObservableCollection<double>([]),
                    GeometrySize = 5,
                    LineSmoothness = 0,

                    Fill = null
                }
            };
            Series1 = new ISeries[]
            {
                new LineSeries<double>
                { Name = "Voltage",
                    Values = new ObservableCollection<double>([]),
                    LineSmoothness = 1,
                    GeometrySize = 15,
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.Red,
                        StrokeCap = SKStrokeCap.Round,
                        StrokeThickness = 2
                    },
                            Fill = null

                }
                };
            Series2 = new ISeries[]
            {
                new LineSeries<double>
                { Name = "Current",
                        Values = new ObservableCollection<double> ([]),
                    LineSmoothness = 0,
                    GeometrySize = 10,
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.Orange,
                        StrokeCap = SKStrokeCap.Butt,
                        StrokeThickness = 2,
                    },
                        Fill = null
                }
            };
            Series3 = new ISeries[]
            {
                new ColumnSeries<double>
                {
                        Name = "",
                        Values = new double[] {}
                }
            };
            Title = new LabelVisual
            {
                Text = "PowerFactor",
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(20),
                Paint = new SolidColorPaint(SKColors.DarkSlateGray)
            };

            Title1 = new LabelVisual
            {
                Text = "Voltage",
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(20),
                Paint = new SolidColorPaint(SKColors.DarkSlateGray)
            };

            Title2 = new LabelVisual
            {
                Text = "Current",
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(20),
                Paint = new SolidColorPaint(SKColors.DarkSlateGray)
            };
            Title3 = new LabelVisual
            {
                Text = "KWH",
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(20),
                Paint = new SolidColorPaint(SKColors.DarkSlateGray)
            };

            GenerateReportViewModel = new GenerateReportVM();
            Instance = this;
            Today = DateTime.Today.ToString("dd-MM-yyyy");

            using (var connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<MeterDetails>();
                connection.CreateTable<MeterData>();
                connection.CreateTable<FilePath>();
                connection.CreateTable<MinMaxValues>();
                connection.CreateTable<EnergyData>();
               
            }

            GenerateReportCommand = new DelegateCommand(OpenGenerateReport);
            MeterInfoCommand = new DelegateCommand(OpenMeter);
            RefreshCommand = new DelegateCommand(RefreshButtonClicked);

            _pollingTimer = new System.Timers.Timer(2000);
            _pollingTimer.Elapsed += OnPollingTimerElapsed;
            _pollingTimer.AutoReset = true;
            _pollingTimer.Enabled = true;
         
            Meters = GetMeterNames();

            if (Meters == null || !Meters.Any())
            {
                SelectedMeter = "";
            }
            else
            {
               SelectedMeter = Meters.First();
            }

            if (XAxes == null)
            {
                XAxes = new[]
              {
                    new Axis
                    {
                        Name = "Date",
                        Labels = [],
                        Labeler = val => val.ToString(), // Customize as needed
                        //LabelsRotation = -45,
                        SeparatorsAtCenter = true,
                        TicksAtCenter = true,
                        ForceStepToMin = false,
                        MinStep = 1
                    }
                };
            }

            threads = new List<Thread>();
            createThreads();
            UpdateChartData();
            CalculateAndSaveMinMaxValues(SelectedMeter);
            StartDateCheckTimer();
        }
       
        private void RefreshButtonClicked()
        {
            // threads = new List<Thread>();
            Meters = GetMeterNames();
            //UpdateChartData();
            //CalculateAndSaveMinMaxValues(SelectedMeter);
            createThreads();
            //StartMeterPolling();
            //writeRandomData();
        }

        public void createThreads()
        {
            List<MeterDetails> meterDetailsList = new List<MeterDetails>();
           
            using (var connection = new SQLiteConnection(App.databasePath))
            {
                meterDetailsList = connection.Table<MeterDetails>().ToList();
            }

            if (meterDetailsList == null || !meterDetailsList.Any())
            {
                return;
            }

            if (threads.Count != meterDetailsList.Count)
            {
                int offset = meterDetailsList.Count - threads.Count;
            }

            List<string> urls = new List<string>();
            
            foreach (var meter in meterDetailsList)
            {
                urls.Add($"{meter.IPAddress}, {meter.MeterName}");
                try
                {
                    Thread thread1 = new Thread(() => FetchAndUpdateMeterData(meter.IPAddress, meter.MeterName))
                    {
                        IsBackground = true
                    };
                    if (!thread1.IsAlive) { thread1.Start(); threads.Add(thread1); }
                }
                catch { }
                //string mn = $"{meter.MeterName}";
            }

        }

        private double ParseValue(string value)
        {
            // Check if the value is a valid double, otherwise return 0
            double result;
            if (double.TryParse(value, out result))
            {
                return result;
            }
            else
            {
                // If it's not a valid number, return 0
                return 0;
            }
        }

        public async void FetchAndUpdateMeterData( string ipAddr, string meter)
        {
            //double prevEnergy = 0;
            while (true)
            {
                try
                {
                    using (var connection = new SQLiteConnection(App.databasePath) )
                    {
                       
                        string startDate = DateTime.Now.ToString("dd-MM-yyyy");
                        var meterDetails = connection.Table<MeterDetails>().Where(v => v.IPAddress == ipAddr && v.MeterName == meter).Distinct().ToList().FirstOrDefault();
                        // Trace.WriteLine("thread started");
                      // meterDetails.MeterName = "Main";
                        var savedData = connection.Table<MinMaxValues>()
                            .Where(d => d.MeterName == meterDetails.MeterName && d.Date == startDate)
                            .FirstOrDefault();

                        if (meterDetails == null)
                        {
                            // Trace.WriteLine("null");
                            return; // Exit if no meter details found
                        }
                        else
                        {
                            //var randomData = writeRandomData1();
                            string ipAddress = ipAddr;
                           // Trace.WriteLine(ipAddr);
                            var response1 = await _httpClient.GetAsync("http://" + ipAddress + "/readdata");

                            await Task.Delay(10000);
                            // Trace.WriteLine("thread waited");
                          var response = await _httpClient.GetAsync("http://"+ ipAddress + "/getdata");
                            //Trace.WriteLine(response);
                           //var response = await _httpClient.GetAsync(ipAddress) ;
                             //Trace.WriteLine(ipAddress);

                           if (response.IsSuccessStatusCode)
                             //if (true)
                            {
                                string responseBody = await response.Content.ReadAsStringAsync();
                                //Trace.WriteLine(responseBody);
                               //string responseBody = GenerateRandomMeterData();
                                //string  responseBody = "client {id: 0,12.34567,inf,0.8765,923.45,ovf,}";
                                //Trace.WriteLine(responseBody);
                                responseBody = responseBody
                                                    .Replace("client", "")
                                                    .Replace("Client", "")
                                                    .Replace("Client ", "")
                                                    .Replace("client ", "")// Remove the word "client"
                                                    .Replace("id: 0", "")                // Remove "id: 0"
                                                    .Replace("id:0", "")                 // Remove "id:0" (no space)
                                                    .Replace("id:  0", "")               // Remove "id:  0" (with extra spaces)
                                                    .Replace("{,", "")                   // Remove "{,"
                                                    .Replace(",}", "")
                                                    .Replace("{,", "")
                                                    .Replace(", }", "")
                                                    .Trim();
                                //"client {,222,44.03,333,66777.8999,2345678}",
                                //"{id: 0,222,44.667,0.888,67844,ovf}",
                                //"{id: 0, 222, 44.667, 0.888, 67844,23456}"

                                var words = responseBody.Split(new char[] { ',', ' ', '{', '}', '"', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

                               // foreach (var word in words) { Trace.WriteLine(word); }

                                MeterData meterData = new MeterData
                                {
                                    MeterName = meterDetails.MeterName,
                                    //Voltage = Math.Round(Math.Abs(ParseValue(words[0])), 2),  // Convert to absolute value
                                    //Current = Math.Round(Math.Abs(ParseValue(words[1])), 2),  // Convert to absolute value
                                    //PF = Math.Round(Math.Max(0, Math.Min(ParseValue(words[2]), 1)), 2),
                                    Voltage = Math.Round(ParseValue(words[0]), 2),
                                    Current = Math.Round(ParseValue(words[1]), 2),
                                    PF = Math.Round(ParseValue(words[2]), 2),
                                    KWH = Math.Round(ParseValue(words[3]), 2),
                                    KVA = Math.Round(ParseValue(words[4]), 2),
                                    Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                                    Date = DateTime.Now.ToString("dd-MM-yyyy"),
                                };
                                //Trace.WriteLine(meterData.Voltage);
                                //Trace.WriteLine(meterData.Current);
                                //Trace.WriteLine(meterData.PF);
                                //Trace.WriteLine(meterData.KWH);
                                //Trace.WriteLine(meterData.KVA);
                                if (meterData.Voltage < 0 || meterData.Current < 0 || meterData.PF < 0 || meterData.PF > 1)
                                {
                                    return; 
                                }

                                if (savedData != null)
                                    {
                                        // Update only if new values are higher or lower
                                        if (meterData.Voltage >= savedData.MaxVoltage)
                                        {  // Trace.WriteLine(meterData.Voltage+ "is meter voltage"); 
                                           // Trace.WriteLine(savedData.MaxVoltage + "is max voltage"); 
                                            savedData.MaxVoltage = meterData.Voltage;
                                            savedData.MaxVoltageTime = DateTime.Now.ToString(" HH:mm");
                                        }
                                        if (meterData.Current >= savedData.MaxCurrent) { savedData.MaxCurrent = meterData.Current; savedData.MaxCurrentTime = DateTime.Now.ToString(" HH:mm"); }
                                        if (meterData.PF >= savedData.MaxPF) { savedData.MaxPF = meterData.PF; savedData.MaxPFTime = DateTime.Now.ToString(" HH:mm"); }
                                        if (meterData.KVA >= savedData.MaxTHD) { savedData.MaxTHD = meterData.KVA; savedData.MaxTHDTime = DateTime.Now.ToString(" HH:mm"); }
                                        if (meterData.Voltage <= savedData.MinVoltage) { savedData.MinVoltage = meterData.Voltage; savedData.MinVoltageTime = DateTime.Now.ToString(" HH:mm"); }
                                        if (meterData.Current <= savedData.MinCurrent) { savedData.MinCurrent = meterData.Current; savedData.MinCurrentTime = DateTime.Now.ToString(" HH:mm"); }
                                        if (meterData.PF <= savedData.MinPF) { savedData.MinPF = meterData.PF; savedData.MinPFTime = DateTime.Now.ToString(" HH:mm"); }
                                        if (meterData.KVA <= savedData.MinTHD) { savedData.MinTHD = meterData.KVA; savedData.MinTHDTime = DateTime.Now.ToString(" HH:mm"); }

                                    if (meterData.KWH != 0)
                                        {
                                            savedData.MaxEnergy += (meterData.KWH - savedData.MinEnergy);
                                            savedData.MinEnergy = meterData.KWH;
                                        }

                                        connection.Update(savedData);
                                    }
                                  else if (meterData.KWH != 0)
                              //else
                                    {
                                        savedData = new MinMaxValues
                                        {
                                            MeterName = meterDetails.MeterName,
                                            MaxVoltage = meterData.Voltage,
                                            MinVoltage = meterData.Voltage,
                                            MaxCurrent = meterData.Current,
                                            MinCurrent = meterData.Current,
                                            MaxPF = meterData.PF,
                                            MinPF = meterData.PF,
                                            MaxTHD = meterData.KVA,
                                            MinTHD = meterData.KVA,
                                            Date = startDate,
                                            Timestamp = DateTime.Now.ToString("dd-MM-yyyy HH:mm"),
                                            MaxEnergy = 0,
                                            MinEnergy = meterData.KWH,
                                            MaxVoltageTime = DateTime.Now.ToString(" HH:mm"),
                                            MaxCurrentTime = DateTime.Now.ToString(" HH:mm"),
                                            MaxPFTime = DateTime.Now.ToString(" HH:mm"),
                                            MinVoltageTime = DateTime.Now.ToString(" HH:mm"),
                                            MinCurrentTime = DateTime.Now.ToString(" HH:mm"),
                                            MinPFTime = DateTime.Now.ToString(" HH:mm"),
                                            MaxTHDTime = DateTime.Now.ToString(" HH:mm"),
                                            MinTHDTime = DateTime.Now.ToString(" HH:mm")
                                        };
                                        // Trace.WriteLine("craeted entry minmax");
                                        connection.Insert(savedData);
                                    }
                                //else
                                //{
                                //    savedData = new MinMaxValues
                                //    {
                                //        MeterName = meterDetails.MeterName,
                                //        MaxVoltage = meterData.Voltage,
                                //        MinVoltage = meterData.Voltage,
                                //        MaxCurrent = meterData.Current,
                                //        MinCurrent = meterData.Current,
                                //        MaxPF = meterData.PF,
                                //        MinPF = meterData.PF,
                                //        Date = startDate,
                                //        Timestamp = DateTime.Now.ToString("dd-MM-yyyy HH:mm"),
                                //        MaxEnergy = 0,
                                //        MinEnergy = meterData.KWH,
                                //        MaxVoltageTime = DateTime.Now.ToString(" HH:mm"),
                                //        MaxCurrentTime = DateTime.Now.ToString(" HH:mm"),
                                //        MaxPFTime = DateTime.Now.ToString(" HH:mm"),
                                //        MinVoltageTime = DateTime.Now.ToString(" HH:mm"),
                                //        MinCurrentTime = DateTime.Now.ToString(" HH:mm"),
                                //        MinPFTime = DateTime.Now.ToString(" HH:mm")
                                //        MaxTHD = meterData.KVA,
                                //        MinTHD = meterData.KVA,
                                //        MaxTHDTime = DateTime.Now.ToString(" HH:mm"),
                                //        MinTHDTime = DateTime.Now.ToString(" HH:mm")
                                //    };
                                //    // Trace.WriteLine("craeted entry minmax");
                                //    connection.Insert(savedData);
                                //}

                                if (SelectedMeter == meterData.MeterName)
                                    {
                                        //UI update
                                        List<double> lastvalues = getlastKWh(7);
                                        lastvalues[lastvalues.Count - 1] = savedData.MaxEnergy;
                                        Series3[0].Values = lastvalues;

                                        MaxVoltage = $"{savedData.MaxVoltage} V at {savedData.MaxVoltageTime}\n";
                                        MaxCurrent = $"{savedData.MaxCurrent} A at {savedData.MaxCurrentTime}\n";
                                        MaxPF = $"{savedData.MaxPF}  at {savedData.MaxPFTime}\n";
                                        MinVoltage = $"{savedData.MinVoltage} V at {savedData.MinVoltageTime}\n";
                                        MinCurrent = $"{savedData.MinCurrent} A at {savedData.MinCurrentTime}\n";
                                        MinPF = $"{savedData.MinPF}  at {savedData.MinPFTime}\n";
                                    }

                                    var existingEntries = connection.Table<MeterData>()
                                    .Where(d => d.MeterName == meterData.MeterName && d.Date == meterData.Date)
                                    .OrderBy(d => d.Timestamp)
                                    .ToList();

                                    if (existingEntries.Count >= 500)
                                    {
                                        // Delete the oldest entry
                                        var oldestEntry = existingEntries.First();
                                        connection.Delete(oldestEntry);
                                    }

                                    // Insert the new entry
                                    connection.Insert(meterData);
                                   
                                await Task.Run(() =>
                                {
                                    if (SelectedMeter == meterData.MeterName)
                                    {
                                        UpdateChartData();
                                    }
                                });

                                //connection.Insert(meterData);
                                //Trace.WriteLine($"Inserted data: {meterData.PF}, {meterData.Voltage}, {meterData.Current}, {meterData.KWH}, {meterData.KVA}");
                            }
                                //Trace.WriteLine($"{meter.MeterName} - {meter.IPAddress} - {meter.PollingInterval}");
                                await Task.Delay(5000);
                            }
                        }
                    
                }
                catch (Exception ex)
                {
                   // Trace.WriteLine($"Error fetching or updating data for meter: {ex.Message}");
                }

            }
        }

        private static void EnergyValue()
        {
            try
            {
               // Trace.WriteLine("energyvalue");
                SQLiteConnection connection = new SQLiteConnection(App.databasePath);
                string date = DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy");
                MinMaxValues weldreport = connection.Table<MinMaxValues>()
                                    .Where(d => d.MeterName == "Welding1" && d.Date == date)
                                    .OrderByDescending(d => d.Timestamp).FirstOrDefault();

                double? weldkwh = weldreport?.MaxEnergy;
                double? compresskwh = connection.Table<MinMaxValues>()
                                    .Where(d => d.MeterName == "Compressor" && d.Date == date)
                                    .OrderByDescending(d => d.Timestamp).FirstOrDefault()?.MaxEnergy;
                double? weld2kwh = connection.Table<MinMaxValues>()
                                    .Where(d => d.MeterName == "Welding2" && d.Date == date)
                                    .OrderByDescending(d => d.Timestamp).FirstOrDefault()?.MaxEnergy;
                MinMaxValues bracereport = connection.Table<MinMaxValues>()
                                    .Where(d => d.MeterName == "Brace" && d.Date == date)
                                    .OrderByDescending(d => d.Timestamp).FirstOrDefault();
                double? bracekwh = bracereport?.MaxEnergy;
                double? robotkwh = connection.Table<MinMaxValues>()
                                    .Where(d => d.MeterName == "Robot" && d.Date == date)
                                    .OrderByDescending(d => d.Timestamp).FirstOrDefault()?.MaxEnergy;
                double? weld3kwh = connection.Table<MinMaxValues>()
                                    .Where(d => d.MeterName == "Welding3" && d.Date == date)
                                    .OrderByDescending(d => d.Timestamp).FirstOrDefault()?.MaxEnergy;
                double? ffkwh = connection.Table<MinMaxValues>()
                                    .Where(d => d.MeterName == "FuelFiller" && d.Date == date)
                                    .OrderByDescending(d => d.Timestamp).FirstOrDefault()?.MaxEnergy;
                double? mainkwh = connection.Table<MinMaxValues>()
                          .Where(d => d.MeterName == "Main" && d.Date == date)
                          .OrderByDescending(d => d.Timestamp)
                          .Select(d => d.MaxEnergy) // Selects the MaxEnergy directly
                          .FirstOrDefault();

                //double Welding1KWh = weldkwh - compresskwh;
                //Trace.WriteLine(Welding1KWh);
                //double Welding2KWh = weld2kwh - (bracekwh + robotkwh);
                //Trace.WriteLine(Welding2KWh);
                double Welding1KWh = weldkwh.HasValue ? (weldkwh.Value - (compresskwh ?? 0)) : 1;
               // Trace.WriteLine(Welding1KWh);
                double Welding2KWh = weld2kwh.HasValue ? (weld2kwh.Value - ((bracekwh ?? 0) + (robotkwh ?? 0))) : 1;
               // Trace.WriteLine(Welding2KWh);

                List<EnergyData> energyDataList = new List<EnergyData>
            {
                new EnergyData { MeterName = "Welding1", KWH = Welding1KWh, Date = date },
                new EnergyData { MeterName = "Compressor", KWH = compresskwh, Date = date },
                new EnergyData { MeterName = "Welding2", KWH = Welding2KWh, Date = date },
                new EnergyData { MeterName = "Welding3", KWH = weld3kwh, Date = date },
                new EnergyData { MeterName = "Brace", KWH = bracekwh, Date = date },
                new EnergyData { MeterName = "Robot", KWH = robotkwh, Date = date },
                new EnergyData { MeterName = "FuelFiller", KWH = ffkwh, Date = date },
                new EnergyData { MeterName = "Main", KWH = mainkwh, Date = date }
            };

                foreach (var energyData in energyDataList)
                {
                    connection.Insert(energyData);
                }
            }
            catch (Exception ex) { }
        }

        private List<double> getlastKWh(int entryCount)
        {
            List<double> KwhValues = new() { };
            using (var connection = new SQLiteConnection(App.databasePath))
            {
                XAxes = new[]
              {
                    new Axis
                    {
                        Name = "Date",
                        Labels = [],
                        Labeler = val => val.ToString(), // Customize as needed
                        //LabelsRotation = -45,
                        SeparatorsAtCenter = true,
                        TicksAtCenter = true,
                        ForceStepToMin = false,
                        MinStep = 1
                    }
                };
                for (int k = 0; k < entryCount; k++)
                {
                   // Trace.WriteLine("here");
                    //MeterData freport = new(), lreport = new MeterData();
                    string date = DateTime.Now.AddDays(-1 * k).ToString("dd-MM-yyyy");

                    var savedData = connection.Table<MinMaxValues>().Where(v => v.Date.StartsWith(date) && v.MeterName == SelectedMeter).ToList().FirstOrDefault();
                    //freport = reports.FirstOrDefault();
                    //lreport = reports.LastOrDefault();
                    
                    if (savedData != null)
                    {
                       // Trace.WriteLine(savedData.MaxEnergy);
                        KwhValues.Add(savedData.MaxEnergy);
                        XAxes[0].Labels.Add(DateTime.Now.Date.AddDays(-k).ToString("dd/MM"));
                        //Trace.WriteLine(lreport.KWH - freport.KWH);
                        //lineSeries.Values.Add(7d);
                        //Series3[0].Values = new double[] {};
                        //  XAxes[entryCount - 1 - k] = date;
                    }                  
                }
                Series3[0].Values = KwhValues;
                
            }
            return KwhValues;
        }

        public List<string> GetMeterNames()
        {
            var meterNames = new List<string>();
            try
            {
                using (var connection = new SQLiteConnection(App.databasePath))
                {
                    var meterDetailsList = connection.Table<MeterDetails>().ToList();
                    var validMeterNames = meterDetailsList.Where(m => !string.IsNullOrWhiteSpace(m.MeterName)).Select(m => m.MeterName).Distinct().ToList();
                    meterNames.AddRange(validMeterNames);
                }
                foreach (var MeterName in meterNames)
                {
                    //Trace.WriteLine(MeterName);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error fetching meter names: {ex.Message}");
            }
            return meterNames;
        }

        private void OnPollingTimerElapsed(object sender, ElapsedEventArgs e)
        {
           Meters = GetMeterNames();
           //UpdateChartData();          
        }

        private System.Timers.Timer reportTimer;
        private bool isReportGenerated = false;
        
        public void StartDateCheckTimer()
        {
            reportTimer = new System.Timers.Timer(GetTimeUntilNextRun().TotalMilliseconds);
            reportTimer.Elapsed += async (sender, e) => await OnReportTimerElapsed();
            reportTimer.AutoReset = false;  // Ensure it runs only once and resets later
            reportTimer.Start();
        }

        private TimeSpan GetTimeUntilNextRun()
        {
            DateTime now = DateTime.Now;
            DateTime nextRun = now.Date.AddHours(6); // Run at 6:00 AM

            if (now > nextRun)
                nextRun = nextRun.AddDays(1); // Move to next day's 6:00 AM

            return nextRun - now;
        }

            private async Task OnReportTimerElapsed()
            {
                string today = DateTime.Today.ToString("dd-MM-yyyy");

                if (Today != today && DateTime.Now.Hour == 13 && DateTime.Now.Minute == 5)
                {
                    Today = today;
                    isReportGenerated = false;
                }

                if (!isReportGenerated)
                {
                    isReportGenerated = true;
                    await RunReportTasksInBackground();
                }
                StartDateCheckTimer(); // Restart timer
            }

        private async Task RunReportTasksInBackground()
        {
            try
            {
                GenerateReportViewModel.PreventSleepMode(); // Ensure the system does not sleep

                // Run the tasks in parallel without blocking the UI
                await Task.WhenAll(
                    Task.Run(() => EnergyValue()),
                    Task.Run(() => GenerateReportViewModel.EnergyDataReport()),
                    Task.Run(() => GenerateReportViewModel.Sendmail())
                );
            }
            catch (Exception ex)
            {
                GenerateReportViewModel.LogError($"Report generation failed: {ex.Message}");
            }
            finally
            {
                GenerateReportViewModel.AllowSleepMode(); // Restore normal sleep behavior
            }
        }

        private async void Reportsdownload()
        {
            EnergyValue();
            GenerateReportViewModel.EnergyDataReport();
            await GenerateReportViewModel.Sendmail();

        }

        private void UpdateChartData()
        {
            //if (Today != DateTime.Now.ToString("dd-MM-yyyy"))
            //{
            //    Today = DateTime.Today.ToString("dd-MM-yyyy");
            //    Thread thread = new Thread(() => Reportsdownload())
            //    {
            //        IsBackground = true
            //    };
            //    thread.Start();
            //}

            try
            {
                if (!string.IsNullOrEmpty(SelectedMeter))
                {
                    using (var connection = new SQLiteConnection(App.databasePath))
                    {
                        var meterDataList = connection.Table<MeterData>()
                            .Where(s => s.MeterName == SelectedMeter && s.Date == Today)
                            
                            .TakeLast(7)
                            .ToList();

                        if (meterDataList.Count == 0)
                        {
                           // Trace.WriteLine("No data available to display.");
                            Series[0].Values = new ObservableCollection<double>([]);
                            Series1[0].Values = new ObservableCollection<double>([]);
                            Series2[0].Values = new ObservableCollection<double>([]);
                            Series3[0].Values = new ObservableCollection<double>([]);
                            return;
                        }

                        double[] voltageValues = new double[meterDataList.Count];
                        double[] currentValues = new double[meterDataList.Count];
                        var pfValues = new double[meterDataList.Count];
                        //var kwhValues = new double[7];


                        ((ColumnSeries<double>)Series3[0]).Values = getlastKWh(7);

                        var timeLabels = new string[meterDataList.Count];

                        // Populate arrays with values
                        for (int i = 0; i < meterDataList.Count; i++)
                        {
                            var data = meterDataList[i];
                            voltageValues[i] = data.Voltage;
                            currentValues[i] = data.Current;
                            pfValues[i] = data.PF;

                            //kwhValues[i] = data.KWH;
                        }
                        ((LineSeries<double>)Series[0]).Values = pfValues;
                        ((LineSeries<double>)Series1[0]).Values = voltageValues;
                        ((LineSeries<double>)Series2[0]).Values = currentValues;
                    }
                }
            }
            catch (Exception ex)
            {
               // Trace.WriteLine($"Error updating chart data: {ex.Message}");
            }
        }

        private void CalculateAndSaveMinMaxValues(string meterName)
        {
            var currentDate = DateTime.Now.Date;
            var startDate = currentDate.ToString("dd-MM-yyyy");

            try
            {
                using (var connection = new SQLiteConnection(App.databasePath))
                {
                    // Get saved data for the current date
                    var savedData = connection.Table<MinMaxValues>()
                        .Where(d => d.MeterName == meterName && d.Date == startDate)
                        .FirstOrDefault();

                    if (savedData != null)
                    {
                        MaxVoltage = $"{savedData.MaxVoltage} V at {savedData.MaxVoltageTime}\n";
                        MaxCurrent = $"{savedData.MaxCurrent} A at {savedData.MaxCurrentTime}\n";
                        MaxPF = $"{savedData.MaxPF}  at {savedData.MaxPFTime}\n";
                        MinVoltage = $"{savedData.MinVoltage} V at {savedData.MinVoltageTime}\n";
                        MinCurrent = $"{savedData.MinCurrent} A at {savedData.MinCurrentTime}\n";
                        MinPF = $"{savedData.MinPF}  at {savedData.MinPFTime}\n";
                    }
                    else
                    {
                        MaxVoltage = "";
                        MaxCurrent = "";
                        MaxPF = "";
                        MinVoltage = "";
                        MinCurrent = "";
                        MinPF = "";
                    }
                }
            }
            catch { }
        }

        private void OpenGenerateReport()
        {
            var generateReportWindow = new GenerateReport();
            generateReportWindow.Show();
        }

        private void OpenMeter()
        {
            var meterInfoWindow = new MeterType();
            meterInfoWindow.Show();
        }

        void writeRandomData1()
        {
            Random random = new Random();
            Trace.WriteLine("started generating random data");

            // This method will generate random float values and process them like the HTTP response data
            List<string> simulatedData = new List<string>();

            // Simulate generating random float data for testing
            for (int b = 0; b < 10; b++)
            {
                string timestamp = DateTime.Now.AddDays(-1 * b).ToString("HH:mm");
                string date = DateTime.Now.AddDays(-1 * b).ToString("dd-MM-yyyy");

                for (float i = 0; i < 1000; i++)
                {
                    // Generate random float values
                    float voltage = (float)(random.NextDouble() * 100);  // Simulating float between 0 and 100
                    float current = (float)(random.NextDouble() * 50);  // Simulating float between 0 and 50
                    float pf = (float)(random.NextDouble() * 1);         // Simulating float between 0 and 1
                    float kwh = i * i;
                    float kva = i * i;

                    // Simulate a response body similar to the one you would get from the HTTP request
                    string response = $"client {{id: 0,{voltage},{current},{pf},{kwh},{kva},}}";
                    simulatedData.Add(response);
                }
            }

            // Now, trigger FetchAndUpdateMeterData with simulated data instead of the HTTP request
            foreach (var data in simulatedData)
            {
                // Replace FetchAndUpdateMeterData's HTTP request with the random data response
                //FetchAndUpdateMeterData(data);
            }
        }

        private string GenerateRandomMeterData()
        {
            Random random = new Random();
            float voltage = (float)(random.NextDouble() * (415 - 375) + 375); // Voltage between 375V-415V
            float current = (float)(random.NextDouble() * (1500 - 800) + 800); // Current between 800A-1500A
            float pf = (float)(random.NextDouble()); // Power factor between 0 and 1
            float kwh = (float)(random.NextDouble() * 1000); // KWH energy
            float kva = (float)(random.NextDouble() * 1000); // KVA rating

            return $"client {{id: 0,{voltage},{current},{pf},{kwh},{kva},}}";
        }

        void writeRandomData()
        {
            Random random = new Random();
            Trace.WriteLine("started");
            using (var connection = new SQLiteConnection(App.databasePath))
            {
                var meterDetailsList = connection.Table<MeterDetails>().ToList();
                
                foreach (var meter in meterDetailsList) 
                {
                    for (int b=0; b<10; b++)
                    {
                        Trace.WriteLine($"{meter.MeterName}");
                        string timestamp = DateTime.Now.AddDays(-1*b).ToString("HH:mm");
                        string date = DateTime.Now.AddDays(-1*b).ToString("dd-MM-yyyy");
                        for (float i = 0; i < 1000; i++)
                        {
                            MeterData meterData = new MeterData
                            {
                                MeterName = meter.MeterName,
                                Voltage = random.Next(375, 415),
                                Current = random.Next(800, 1500),
                                PF = random.NextDouble(),
                                KWH = i * i,
                                KVA = i * i,
                                Timestamp = timestamp,
                                Date = date,
                            };

                            //connection.Insert(meterData);
                        }
                    }
                }
                //return meterData;
                //Trace.WriteLine("completed");
            }
            
        }
    }
}