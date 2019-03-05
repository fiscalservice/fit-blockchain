/**
 * This is the controller for the splashScreen
 * @namespace AssetTrackPOC.Droid.Splash.SplashActivity
 * @type controller
 * @author David Amalfitano<damalfitano@deloitte.com>
 */
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using AssetTrackPOC.Droid;

namespace com.damalfitano.AssetTrackPOC
{
    [Activity(Theme = "@style/MyTheme.Splash", Icon = "@drawable/logo", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
            Log.Debug(TAG, "SplashActivity.OnCreate");
        }

        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();
            Task startupWork = new Task(() => { StartActivity(new Intent(Application.Context, typeof(MainActivity))); });
            startupWork.Start();
        }

        // Prevent the back button from canceling the startup process
        public override void OnBackPressed() { }
    }
}