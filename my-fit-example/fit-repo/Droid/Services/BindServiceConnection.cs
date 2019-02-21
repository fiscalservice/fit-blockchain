/**
 * This is the Binder that allows the service to stay active in the background.
 * @namespace AssetTrackPOC.Droid.Services.BindServiceConnection
 * @type Binder
 * @author David Amalfitano <damalfitano@deloitte.com>
 **/
using Android.OS;

namespace AssetTrackPOC.Droid.Services
{
    public class BindServiceConnection : Binder
    {

        BackgroundService mBackgroundService;
        public BindServiceConnection(BackgroundService _backgroundService)
        {
            this.mBackgroundService = _backgroundService;
        }

        public BackgroundService GetBackgroundService(){
            return mBackgroundService;
        }
    }
}

