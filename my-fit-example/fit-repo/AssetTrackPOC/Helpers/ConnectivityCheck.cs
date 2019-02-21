using System;
using System.Threading.Tasks;
using AssetTrackPOC.Controllers;
using Plugin.Connectivity.Abstractions;
using Xamarin.Forms;

namespace AssetTrackPOC.Helpers
{
    public class ConnectivityCheck
    {
        public ConnectivityCheck(){}

        public static async Task<bool> Current_ConnectivityChanged()
        {
            if (!e.IsConnected)
            {
                return false;
               // await DisplayAlert("Error", Constants.CHECK_CONNECTION, Constants.BUTTON_POS);
            }
            return true;
        }

    }
}
