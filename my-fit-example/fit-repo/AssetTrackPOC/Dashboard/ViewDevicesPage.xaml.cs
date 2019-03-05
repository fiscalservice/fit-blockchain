/**
 * This is the controller is part of the dashboard containing the list of devices the user has in possesion
 * @type controller
 * @author David Amalfitano <damalfitano@deloitte.com>
 * TODO: remove the costCode column temporary
 **/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AssetTrackPOC.Controllers;
using AssetTrackPOC.CustomCell;
using AssetTrackPOC.Helpers;
using AssetTrackPOC.Model;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace AssetTrackPOC.Dashboard
{
    public partial class ViewDevicesPage : ContentPage
    {
        //Member variables
        string acctType;
        List<MobileDevice> deviceList = new List<MobileDevice>();
        string usernameKey;
        Label noDeviceInListLabel;
        string costCode;

        public ViewDevicesPage()
        {
            InitializeComponent();
            Title = "View My Devices";
            acctType = (string)LogInResponseData.GetApplicationCurrentProperty(Constants.ACCOUNT_TYPE);
            usernameKey = Application.Current.Properties[Constants.USERNAME_KEY] as string;
            costCode = Application.Current.Properties[Constants.USER_COSTCODE] as string;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            PullListOfDevices(usernameKey);
            noDeviceInListLabel = ObjectCreatorHelper.createLabel(13, 40, Color.White, TextAlignment.Center, Color.Gray, "No Active Devices", new Thickness(0, 0, 0, 0));
            noDeviceInListLabel.VerticalTextAlignment = TextAlignment.Center;
            noDeviceInListLabel.HorizontalTextAlignment = TextAlignment.Center;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            deviceList.Clear();
        }

        public async void PullListOfDevices(string username){

            var viewDeviceLv = ObjectCreatorHelper.createListview(false, new DataTemplate(typeof(MyDeviceCell)), 40, false, Color.White);
            viewDeviceLv.Margin = new Thickness(0, 0, 16, 0);

            var responsePull = await ServiceClient.Instance.getDevices(username);

            if (responsePull.ResponseCode != 200)
            {
                await DisplayAlert(string.Empty, "Error connecting to server", "Ok");
                //loadingOverlay.IsVisible = false;
               // return;
            }
           
            deviceList.Add(new MobileDevice { ID = "AssetId", Name = "Type", UnitCost = "CostCode", DeviceColor = Color.FromHex(Constants.LIST_HEADER_BG_COLOR), OtherLabel = "Status" });

                if (responsePull.ResponseString != null && responsePull.ResponseString != "")
                {
                Debug.WriteLine(responsePull.ResponseString);
                var responseArray = JArray.Parse(responsePull.ResponseString);

                   foreach (JObject o in responseArray.Children<JObject>())
                   {
                      deviceList.Add(new MobileDevice { ID = (string)o["assetId"], Name = "Samsung", UnitCost = costCode, DeviceColor = Color.White, OtherLabel = (string)o["status"] });
                   }
                }

            viewDeviceLv.BeginRefresh();
            viewDeviceLv.ItemsSource = deviceList;//
            viewDeviceLv.EndRefresh();

            ObjectCreatorHelper.addToRootParent(viewDeviceRl, viewDeviceLv,8,8,0);

            if (deviceList.Count == 1)
            {
                ObjectCreatorHelper.addChildToParent(viewDeviceRl, noDeviceInListLabel, viewDeviceLv,0,noDeviceInListLabel.Height,0);
            }
        }
    }
}
