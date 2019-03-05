/**
 * This is the controller for the MainActivity
 * @namespace AssetTrackPOC.Droid.MainActivity
 * @type controller
 * @author David Amalfitano<damalfitano@deloitte.com>
 */
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using ZXing.Mobile;
using AssetTrackPOC.Droid.Services;
using System.IO;
using SQLite;
using AssetTrackPOC.Droid.Model;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Collections.Generic;

namespace AssetTrackPOC.Droid
{
    [Activity(Label = "AssetTrackPOC.Droid", Theme = "@style/MyTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, LaunchMode = LaunchMode.SingleTop, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        string tag = "MainActivity.js";
        BackgroundServiceConnection mBackgroundServiceConnection;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            //Barcode Scanner Ready
            MobileBarcodeScanner.Initialize(Application);

            /* TODO: uncomment to receive IMEI programmatically*/
            //var telephonyManager = (TelephonyManager)GetSystemService(TelephonyService);
            //var id = telephonyManager.Imei; //DeviceId; // retrieving IMEI -- can probably remove
            //if (id != null)
            //{
            //    //Console.Write("Device IMEI ID : "+ id);
            //    Log.Info(tag, "Device IMEI ID *****************************: " + id);
            //}

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
            serviceControl(); // Backgroud service when app is in banckground
            openDBAsync();
            System.Diagnostics.Debug.WriteLine("PLEASE********************************22222222");
        }

        protected override void OnResume()
        {
            base.OnResume();
            //Log.Debug(tag, deviceModel);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                //unpack the intent for the result example:
                data.GetStringExtra("SCAN_RESULT");
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            // Toast.MakeText(this.ApplicationContext, requestCode.ToString(), ToastLength.Short).Show();    
            //global::ZXing.Net.Mobile.Forms.Android.PermissionsHandler.OnRequestPermissionsResult (requestCode, permissions, grantResults);  
            //  global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //global: ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnStop()
        {
            base.OnStop();
            serviceControl(); //Background service when app is closed

        }

        private void serviceControl(){
           
            //Log.Debug(tag, "****SERVICE STOPPED");
            StopService(new Intent(this, typeof(BackgroundService)));
            Intent bgServiceIntent = new Intent(this, typeof(BackgroundService));
            if (mBackgroundServiceConnection == null)
            {
                mBackgroundServiceConnection = new BackgroundServiceConnection();
                BindService(bgServiceIntent, mBackgroundServiceConnection, Bind.AutoCreate); //KEEP testing bind servicing
            }
           // Log.Debug(tag, "****SERVICE STARTED");
            StartService(bgServiceIntent);
        }


        protected async void openDBAsync()
        {
            var databasePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "device_key.db");
            var conn = new SQLiteAsyncConnection(databasePath);
            int result = 0;
            try {
                result = await conn.ExecuteScalarAsync<int>("select count(*) from MobileKey");
            } catch (System.Exception e) {
                System.Diagnostics.Debug.WriteLine(e);
            }

            if (result == 0) {
                storeTheKey();
            } else {
                var db = new SQLiteConnection(databasePath);
                List<MobileKey> resuls = await conn.Table<MobileKey>().ToListAsync();
                foreach (MobileKey i in resuls)
                {
                    System.Diagnostics.Debug.WriteLine(i.keyId);
                    //TODO: grab the key for local storage and usage
                }
            }
        }

        private void storeTheKey()
        {
            //SQLite Db
            var databasePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "device_key.db");
            var db = new SQLiteConnection(databasePath);
            //key
            var web3 = new Nethereum.Web3.Web3("http://ec2-52-91-125-113.compute-1.amazonaws.com:3000/");
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();

            db.CreateTable<MobileKey>();
            var Id = db.Insert(new MobileKey()
            {
                keyId = privateKey
            });
        }
    }//class
    }//namespace