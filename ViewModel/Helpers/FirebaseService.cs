using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Documents;
using System.Windows.Media;
using Google.Cloud.Storage.V1;
//using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;
using Google.Cloud.Firestore;

namespace Project_K.ViewModel.Helpers
{
    public class FirebaseService
    {
        private FirebaseApp _firebaseApp;
        private StorageClient _storageClient;
        private static bool isFirebaseInitialized = false;
        private static FirestoreDb _firestoreDb;
        private DocumentReference emDocRef;
        private static readonly object _lock = new object();

        public FirebaseService()
        {
            if (!isFirebaseInitialized)
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string pathToServiceAccountKey = Path.Combine(appDirectory, "View", "assets", "embediot-76e3b-firebase-adminsdk-fbsvc-8d32d8a0f0.json");

                //string pathToServiceAccountKey = @"D:\\WPF_Projects\\Andon Sys\\Andon -- MQTT -- Firebase\\assets\andon-system-rwa-firebase-adminsdk-55sj1-69c9a3965f.json"; 
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", pathToServiceAccountKey);

                if (FirebaseApp.DefaultInstance == null)
                {
                    _firebaseApp = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(pathToServiceAccountKey)

                    });

                    _firestoreDb = FirestoreDb.Create("embediot-76e3b");
                    _storageClient = StorageClient.Create();
                    Trace.WriteLine("Created");
                    emDocRef = _firestoreDb.Collection("JBM").Document("EnergyMeter");
                    isFirebaseInitialized = true;
                    //Trace.WriteLine("FirebaseApp initialized successfully.");
                }
                else
                {
                    _firebaseApp = FirebaseApp.DefaultInstance;
                    // Trace.WriteLine("FirebaseApp already initialized.");
                    _firestoreDb = FirestoreDb.Create("embediot-76e3b");
                    emDocRef = _firestoreDb.Collection("JBM").Document("EnergyMeter");
                }
            }
            else
            {
                Trace.WriteLine("FirebaseApp already initialized.");
            }
        }

        public static FirestoreDb GetFirestoreDb()
        {
            return _firestoreDb;
        }
      
        public async Task StoreToFirestore(string metername, double Voltage, double Current, double PF, double KWH, double KVA)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string Date = DateTime.Now.ToString("dd-MM-yyyy");
            try
            {              
           
            DocumentReference emDoc = _firestoreDb.Collection("JBM").Document("EnergyMeter");
            DocumentReference MeterDataRef = emDoc.Collection("MeterData").Document(metername+ "_" + timestamp);
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "MeterName", metername },
                { "Voltage", Voltage },
                { "Curent", Current },
                { "PF", PF },
                { "KWH", KWH },
                { "KVA", KVA },
                { "Date", Date },
                { "Time", timestamp }
            };

            MeterDataRef.SetAsync(data).Wait();
            Trace.WriteLine("Firestore MeterData document updated successfully.");
        }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error updating Firestore SectionData: {ex.Message}");
            }
}

    }
}
