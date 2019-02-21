using System;

using Xamarin.Forms;

namespace AssetTrackPOC.Droid
{
    public class MyComponentCallbacks : Java.Lang.Object, Android.Content.IComponentCallbacks
    {
        public void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            // implementation goes here...
        }
        public void OnLowMemory()
        {
            // implementation goes here...
        }
    }
}

