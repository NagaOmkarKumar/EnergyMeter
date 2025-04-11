using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Cloud.Firestore;
using Project_K.Model;
using Project_K.ViewModel.Helpers;
using SQLite;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Project_K.ViewModel
{
    public class MeterVM : ObservableObject
    {
        //private readonly MeterDatabaseHelper _databaseHelper;
        private MeterDetails _meterDetails = new MeterDetails();
        private readonly RoboWorksVM _roboWorksVM;
        private FirebaseService _firebaseService;
        private static FirestoreDb _firestoreDb;
        public MeterDetails MeterDetails
        {
            get => _meterDetails;
            set => SetProperty(ref _meterDetails, value);
        }

        private string _meterName;
        public string MeterName
        {
            get => _meterName;
            set => SetProperty(ref _meterName, value);
        }

        private string _meterData;
        public string MeterData
        {
            get => _meterData;
            set => SetProperty(ref _meterData, value);
        }

        private ObservableCollection<string> _meterNames = new ObservableCollection<string>();
        public ObservableCollection<string> MeterNames
        {
            get => _meterNames;
            set => SetProperty(ref _meterNames, value);
        }

        public ICommand SaveMeterDetailsCommand { get; }
        public ICommand SaveMeterDataCommand { get; }
        public ICommand RemoveMeterDetailsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand DownloadCommand { get; }

        public MeterVM()
        {
            SaveMeterDetailsCommand = new RelayCommand(SaveMeterDetails);
            RemoveMeterDetailsCommand = new RelayCommand(RemoveMeterDetails);
            RefreshCommand = new RelayCommand(LoadMeterNames);
            DownloadCommand = new RelayCommand(Download);
            _firebaseService = new FirebaseService();
            //_firestoreDb = FirestoreDb.Create("andon-system-rwa");
            _firestoreDb = FirebaseService.GetFirestoreDb();
            FirestoreDb firestoreDb = FirebaseService.GetFirestoreDb();

            LoadMeterNames();

            _roboWorksVM = RoboWorksVM.Instance;

        }

        private void LoadMeterNames()
        {
            using (var connection = new SQLiteConnection(App.databasePath))
            {
                //connection.Execute("DELETE FROM MeterDetails");
                var meterDetailsList = connection.Table<MeterDetails>().ToList();
                var validMeterNames = meterDetailsList.Where(m => !string.IsNullOrWhiteSpace(m.MeterName)).Select(m => m.MeterName);
                MeterNames = new ObservableCollection<string>(validMeterNames);
            }
        }

        private void SaveMeterDetails()
        {
            if (MeterDetails == null)
            {
                MessageBox.Show("Meter details cannot be null.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(MeterDetails.MeterName) ||
                string.IsNullOrWhiteSpace(MeterDetails.IPAddress))
            {
                MessageBox.Show("Please enter valid meter details. Check MeterName and IPAddress ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            using (var connection = new SQLiteConnection(App.databasePath))
            {
                var existingMeter = connection.Table<MeterDetails>().FirstOrDefault(m => m.MeterName == MeterDetails.MeterName);
                if (existingMeter != null)
                {
                    MessageBox.Show("A meter with this name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MeterDetails.TimeStamp = DateTime.Now.ToString("dd-MM-yyyy HH:mm");

                connection.Insert(MeterDetails);
                SaveMeterDetailsToFirestore(MeterDetails);
                LoadMeterNames();
                Thread th = new Thread(() => _roboWorksVM.FetchAndUpdateMeterData(MeterDetails.IPAddress, MeterDetails.MeterName))
                {
                    IsBackground = true
                };
                th.Start();
                //_roboWorksVM.createThreads();
            }
        }

        private void Download()
        {
            List<MeterDetails> meterDetailsList;

            try
            {
                using (var connection = new SQLiteConnection(App.databasePath))
                {
                    meterDetailsList = connection.Table<MeterDetails>().ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving meter details: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                meterDetailsList = new List<MeterDetails>();
            }

            string filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"MeterDetails_{DateTime.Now:dd-MM-yyyy_HH_mm}.csv");
            WriteCSV(meterDetailsList, filename);
            MessageBox.Show($"Report saved to {filename}", "Report Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static void WriteCSV(List<MeterDetails> meterDetailsList, string filename)
        {

            //Trace.WriteLine(filename);
            using (var stream = new FileStream(filename, FileMode.Create))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.WriteLine("MeterName,TimeStamp,MeterType,Make,IPAddress,PollingInterval,MeterDescription,ModbusAddress");

                foreach (var meterDetails in meterDetailsList)
                {
                    writer.WriteLine($"{meterDetails.MeterName},{meterDetails.TimeStamp},{meterDetails.MeterType},{meterDetails.Make},{meterDetails.IPAddress},{meterDetails.PollingInterval},{meterDetails.MeterDescription},{meterDetails.ModbusAddress}");
                    //  Trace.WriteLine(meterDetails.ModbusAddress);
                }
            }
            
        }

        private void RemoveMeterDetails()
        {
            if (MeterDetails.MeterName == null)
            {
                MessageBox.Show("No meter details are currently selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string meterNameToRemove = MeterDetails.MeterName.Trim();

            if (string.IsNullOrEmpty(meterNameToRemove))
            {
                MessageBox.Show("Meter name is required to remove.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var connection = new SQLiteConnection(App.databasePath))
                {
                    var existingMeter = connection.Table<MeterDetails>()
                                                  .FirstOrDefault(m => m.MeterName == meterNameToRemove);

                    if (existingMeter == null)
                    {
                        MessageBox.Show("No matching meter found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var result = MessageBox.Show($"Are you sure you want to remove the meter '{meterNameToRemove}'?",
                                         "Confirm Deletion",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        connection.Delete(existingMeter);
                        RemoveMeterDetailsFromFirestore(meterNameToRemove);
                        MessageBox.Show("Meter removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadMeterNames();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void SaveMeterDetailsToFirestore(MeterDetails meterDetails)
        {
            try
            {
                FirestoreDb firestoreDb = FirebaseService.GetFirestoreDb();
                DocumentReference meterRef = firestoreDb.Collection("JBM").Document("EnergyMeter").Collection("MeterDetails").Document(meterDetails.MeterName);
                await meterRef.SetAsync(meterDetails);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error saving meter details to Firestore: {ex.Message}");
            }
        }

        private async void RemoveMeterDetailsFromFirestore(string meterName)
        {
            try
            {
                FirestoreDb firestoreDb = FirebaseService.GetFirestoreDb();
                DocumentReference meterRef = firestoreDb.Collection("JBM").Document("EnergyMeter").Collection("MeterDetails").Document(meterName);
                await meterRef.DeleteAsync();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error removing meter details from Firestore: {ex.Message}");
            }
        }
    }
}