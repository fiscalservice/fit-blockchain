/**
 * This is the service connector that is bind to the background service.
 * @type service connection
 * @author David Amalfitano <damalfitano@deloitte.com>
 **/

using System;
using Android.Content;
using Android.OS;
using Xamarin.Forms;

namespace AssetTrackPOC.Droid.Services
{
    public class BackgroundServiceConnection : Java.Lang.Object, IServiceConnection
    {
        BindServiceConnection binder;

        public BindServiceConnection Binder { get; private set; }

        public BackgroundServiceConnection()
        {
            binder = null;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            binder = service as BindServiceConnection;
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            binder = null;
        }
    }
}

