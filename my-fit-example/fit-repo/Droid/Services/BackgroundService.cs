/**
 * This is the service that sets up the Notification door to allow the loginDevice call to be made in the background.
 * @namespace AssetTrackPOC.Droid.Services.BackgroundService
 * @type Service
 * @author David Amalfitano <damalfitano@deloitte.com>
 **/

using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using Xamarin.Forms;
using AssetTrackPOC;
using AssetTrackPOC.Controllers;
using AssetTrackPOC.Model;
using Plugin.Connectivity;
using System.IO;
using SQLite;
using AssetTrackPOC.Droid.Model;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Collections.Generic;

namespace AssetTrackPOC.Droid.Services
{
    [Service]
    public class BackgroundService : Service
    {
        static readonly string TAG = "X:" + typeof(BackgroundService).Name;
        static readonly int TimerWait = 30 * 1000; // runs every minute
        Timer timer;
        DateTime startTime;
        bool isStarted = false;
        BindServiceConnection binder;
        OpenAppReceiver receiver;
        BackgroundServiceConnection mConnection = new BackgroundServiceConnection();
        Intent currentIntent;

        public override void OnCreate()
        {
            receiver = new OpenAppReceiver();
            IntentFilter filter = new IntentFilter();
            filter.AddAction(Intent.ActionUserPresent);
            filter.AddAction(Intent.ActionScreenOn);
            filter.AddAction(Intent.ActionScreenOff);
            filter.AddAction(Intent.ActionShutdown);
            RegisterReceiver(receiver, filter);
            base.OnCreate();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
           // Log.Debug(TAG, $"OnStartCommand called at {startTime}, flags={flags}, startid={startId}");
            currentIntent = intent;
            if (isStarted)
            {
                TimeSpan runtime = DateTime.UtcNow.Subtract(startTime);
                addNotification();
               // Log.Debug(TAG, $"This service was already started, it's been running for {runtime:c}.");
            }
            else
            {
                startTime = DateTime.UtcNow;
               // this.addNotification();
                //Toast.MakeText(this.ApplicationContext, "START SERVICE", ToastLength.Short).Show();
                //Log.Debug(TAG, $"Starting the service, at {startTime}.");
                timer = new Timer(HandleTimerCallback, startTime, 0, TimerWait);
                isStarted = true;
            }
            return StartCommandResult.NotSticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            // throw new NotImplementedException();
            this.binder = new BindServiceConnection(this);
            return binder;
        }

        public override void OnDestroy()
        {
            timer.Dispose();
            timer = null;
            isStarted = false;

            TimeSpan runtime = DateTime.UtcNow.Subtract(startTime);
            //Log.Debug(TAG, $"Simple Service destroyed at {DateTime.UtcNow} after running for {runtime:c}.");
            base.OnDestroy();
        }

        void HandleTimerCallback(object state)
        {
            TimeSpan runTime = DateTime.UtcNow.Subtract(startTime);
             addNotification(); //TODO: uncomment for notification
                                //   Log.Debug(TAG, $"This service has been running for {runTime:c} (since ${state}).");

            var intent = new Intent("com.djamalfitano.openAppReceiverService");
            string timeStamp = DateTime.Now.ToString();
            intent.PutExtra("timeStamp", timeStamp);
            SendBroadcast(intent);
        }

        private void addNotification()
        {
            // create the pending intent and add to the notification
            var intent = this.PackageManager.GetLaunchIntentForPackage(this.PackageName);// opens app from background ( Can we use this auto open app with alert? )
                                                                                         // intent.AddFlags(ActivityFlags.ClearTop);
                                                                                         //Intent intent = new Intent(this, typeof(BackgroundService));
            PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.UpdateCurrent);
            var lastLogin = "Please login to AssetChain";
            // string lastLogin =  as string;
            if (Xamarin.Forms.Application.Current.Properties.Count > 0)
            {
                if (Xamarin.Forms.Application.Current.Properties[Constants.CURRENT_DATE] != null)
                {
                    lastLogin = "Last login at " + Xamarin.Forms.Application.Current.Properties[Constants.CURRENT_DATE].ToString();
                }
            }
            string deviceModel = DeviceModal;
            Xamarin.Forms.Application.Current.Properties[Constants.DEVICE_MODEL] = deviceModel;

            // create the notification
            Notification.Builder m_notificationBuilder = new Notification.Builder(this)
                    .SetContentTitle("AssetChain")
                    .SetContentText(lastLogin)
                    .SetSmallIcon(Resource.Drawable.logo)
                    .SetContentIntent(pendingIntent);

            Notification notification = m_notificationBuilder.Build();
            notification.Flags = NotificationFlags.AutoCancel;

            // send the notification
            const int NOTIFICATION_ID = 101;
            NotificationManager notificationManager = this.GetSystemService(Context.NotificationService) as NotificationManager;
            StartForeground(NOTIFICATION_ID, notification); // locks notificaion in bar
            notificationManager.Notify(NOTIFICATION_ID, m_notificationBuilder.Build());

        }

        public string DeviceModal
        {
            get
            {
                // read and return current device model
                return string.IsNullOrEmpty(global::Android.OS.Build.Model) ?
                string.Empty :
                global::Android.OS.Build.Model.StartsWith(global::Android.OS.Build.Manufacturer) ?
                global::Android.OS.Build.Model :
                string.Format("{0} {1}", global::Android.OS.Build.Manufacturer, global::Android.OS.Build.Model);
            }
        }


    }

    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "com.djamalfitano.openAppReceiverService" })]
    public class OpenAppReceiver : BroadcastReceiver
    {
        string TAG = "OPENAPPRECEIVER";

        string assetId;
        string username;
        string password;
        public override void OnReceive(Context context, Intent intent)
        {

            if (Xamarin.Forms.Application.Current.Properties.Count > 1)
            {
                 assetId = Xamarin.Forms.Application.Current.Properties[Constants.DEVICE_ADDRESS] as string ?? string.Empty;
                 username = Xamarin.Forms.Application.Current.Properties[Constants.USERNAME_KEY] as string ?? string.Empty;
                 password = Xamarin.Forms.Application.Current.Properties[Constants.PASS_KEY] as string ?? string.Empty;
            }

            string message = "";
            switch (intent.Action)
            {
                case Intent.ActionUserPresent: // Device is unlocked and user enterd the home screen
                    message = "User Is Present";
                    break;
                case Intent.ActionScreenOn: // Device is locked but screen is on
                    message = "Users screen on";
                    break;
                case Intent.ActionScreenOff: //Device is Locked and screen is off
                    message = "User screen off";
                    break;
                case Intent.ActionShutdown:
                    message = "User Shut down";
                    break;
                default:
                    message = "User access App";//User opens device
                    break;

            }
            //Toast.MakeText(context.ApplicationContext, message, ToastLength.Short).Show();
            var timeStampExtra = intent.GetStringExtra("timeStamp") ?? string.Empty;
             if (CrossConnectivity.Current.IsConnected)
             {
                if (message.Equals("User Is Present"))
                {
                    //Toast.MakeText(context.ApplicationContext, "HAS CONNECTION", ToastLength.Short).Show();
                    addNotification(context, message);//intent.GetStringExtra("address"), intent.GetStringExtra("username"), intent.GetStringExtra("password"));
                }
            }
        }

        private async void addNotification(Context _context, string message)
        {
            // TODO: check if user is active and call service
            //Log.Info(TAG, message + "=====");
            if (assetId != null || username != null || password != null)
                {
                    var response = await ServiceClient.Instance.LoginDevice(/*deviceAddress*/assetId, username, password);
                   //Log.Info(TAG, message + "APP DESTROYED");
                }
                else
                {
                    Log.Info(TAG, message + "APP DESTROYED without login info");
                }
        }
    }
}

