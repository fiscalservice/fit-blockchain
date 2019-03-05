using System.Diagnostics;
using AssetTrackPOC.Controllers;
using AssetTrackPOC.Dashboard;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace AssetTrackPOC
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new AssetTrackPOCPage())
            {
                BarBackgroundColor = Color.White, //Color.FromHex("#FF607D8B"),
                BarTextColor = Color.White,
            };  
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            //Debug.WriteLine(TAG,"OnStart");

        }

       

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
