/**
 * This is the service for the AlarmManagerService
 * @namespace AssetTrackPOC.Droid.Services.AlarmManagerService
 * @type service
 * @author David Amalfitano<damalfitano@deloitte.com>
 */
using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;


namespace AssetTrackPOC.Droid.Services
{
    //[BroadcastReceiver]
    // [IntentFilter(new string[] { "android.intent.action.BOOT_COMPLETED" }, Priority = (int)IntentFilterPriority.LowPriority)]
    public class AlarmManagerService : BroadcastReceiver
    {
        public string TAG = "ALARM_MANAGER";
        public Context mContext;
        public AlarmManagerService(Activity _activity)
        {
        }

        public AlarmManagerService(Context context)
        {
            Log.Debug(TAG, "Alarm Manager Created");
            this.setNotification(context, "message content", "titleContent");
        }

        public override void OnReceive(Context context, Intent intent)
        {
            var message = intent.GetStringExtra("message");
            var title = intent.GetStringExtra("title");
            var notIntent = new Intent(context, typeof(MainActivity));
            var contentIntent = PendingIntent.GetActivity(context, 0, notIntent, PendingIntentFlags.CancelCurrent);
            var manager = NotificationManagerCompat.From(context);

            var style = new NotificationCompat.BigTextStyle();
            style.BigText(message);

            //Generate a notification with just short text and small icon
            var builder = new NotificationCompat.Builder(context)
                .SetContentIntent(contentIntent)
                .SetSmallIcon(Resource.Drawable.logo)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetStyle(style)
                .SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis())
                .SetAutoCancel(true);

            var notification = builder.Build();
            manager.Notify(0, notification);
            Log.Debug(TAG, "Alarm Manager started");

        }

        protected void setNotification(Context context, string message, string title)
        {
            Intent alarmIntent = new Intent(context, typeof(AlarmManagerService));
            alarmIntent.PutExtra("message", message);
            alarmIntent.PutExtra("title", title);

            long now = SystemClock.CurrentThreadTimeMillis();
            AlarmManager am = (AlarmManager)context.GetSystemService(Context.AlarmService);
            Intent intent = new Intent(context, this.Class);
            PendingIntent pi = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);

            //PendingIntent pendingIntent = PendingIntent.GetBroadcast(Forms.Context, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);
            AlarmManager alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);

            //TODO: For demo set after 5 seconds.
            alarmManager.Set(AlarmType.RtcWakeup, DateTime.Now.Millisecond + 30000, pi);

        }
    }
}

