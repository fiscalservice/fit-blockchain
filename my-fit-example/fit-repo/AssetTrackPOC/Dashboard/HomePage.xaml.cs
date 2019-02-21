/**
 * This is the controller is part of the dashboard is the main menu navigation for application.
 * @type controller
 * @author David Amalfitano <damalfitano@deloitte.com>
 * TODO: timeout feature
 **/

using System;
using System.Collections.Generic;
using AssetTrackPOC.Controllers;
using AssetTrackPOC.CustomCell;
using AssetTrackPOC.Helpers;
using AssetTrackPOC.Model;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace AssetTrackPOC.Dashboard
{
    public partial class HomePage : ContentPage
    {
        //Member Variables
        string uniqueDeviceAddress;
        //string privateKey;
        string usernameKey;
        string passKey;
        bool logoutFlag = false;
        bool isDisposalAccept = false;
        string deviceModel;

        public HomePage()
        {
            InitializeComponent();
            Title = "Home";
            string acctType = (string)LogInResponseData.GetApplicationCurrentProperty(Constants.ACCOUNT_TYPE);
            uniqueDeviceAddress = Application.Current.Properties[Constants.DEVICE_ADDRESS] as string;
            usernameKey = Application.Current.Properties[Constants.USERNAME_KEY] as string;
            passKey = Application.Current.Properties[Constants.PASS_KEY] as string;
            deviceModel = Application.Current.Properties[Constants.DEVICE_MODEL] as string;

            ListView homeMenuLv = ObjectCreatorHelper.createListview(false, new DataTemplate(typeof(HomeMenuCell)), 80, true, Color.White);
            homeMenuLv.Margin = new Thickness(8, 8, 8, 8);
            homeMenuLv.SeparatorColor = Color.FromHex(Constants.LIST_SEPERATOR_COLOR);
            homeMenuLv.VerticalOptions = LayoutOptions.Fill;

            if(deviceModel != null){
                assetModel.Text = deviceModel;
            }

            if (uniqueDeviceAddress != null){
                currentAssetIdLbl.Text = uniqueDeviceAddress;
            } else {
                currentAssetIdLbl.Text = "UNKNOWN";
            }

            switch (acctType)
            {
                case "1": // eus
                    homeMenuLv.ItemsSource = MenuData.menuListSupport;
                    AccountTypeTitle.Text = "End User Support";
                    break;
                case "0"://pm
                    isDisposalAccept = true;
                    homeMenuLv.ItemsSource = MenuData.menuListManager;
                    AccountTypeTitle.Text = "Property Management";
                    break;
                default:
                    homeMenuLv.ItemsSource = MenuData.menuListEmployee;
                    AccountTypeTitle.Text = "Employee";
                    break;
            }

            homeOptions.Children.Add(homeMenuLv);
            homeMenuLv.ItemSelected += async (sender, e) =>
            {
                loadingOverlayHome.IsVisible = true;
                if (e.SelectedItem == null)
                {
                    // don't do anything if we just de-selected the row
                    return;
                }
                else
                {
                    switch ((e.SelectedItem as MenuObject).menuTitle)
                    {
                        case "Ingest a New Device":
                            //TODO: open up camera layout to add new device before directing user to IngestDevicePage()
                            await Navigation.PushAsync(new IngestDevicePage());
                            break;
                        case "Collect Device":
                             openBarCodeScaner(0); //Removed
                            break;
                        case "Initiate Inventory Check":
                            var result = await DisplayAlert(Constants.INVENTORY_CHECK_TITLE, Constants.INVENTORY_CHECK_CONTENT, Constants.BUTTON_CONFIRM, Constants.BUTTON_CAN);
                            loadingOverlayHome.IsVisible = false;
                            break;
                        case "Accept a Transfer": //  PM / EUS user is accepting transfer into their possesion 
                            openBarCodeScaner(1); 
                            break;
                        case "Disposal": // PM Only
                            openBarCodeScaner(3);
                            break;
                        case "Submit Disposal Request": //EUS Only
                            openBarCodeScaner(2);
                            break;
                        case "View Transfers":
                            await Navigation.PushAsync(new InitiateTransferPage());
                            break;
                        case "View My Devices":
                            await Navigation.PushAsync(new ViewDevicesPage());
                            break;
                        default:
                            await DisplayAlert("Tapped", "Error in code" + " row was selected", Constants.BUTTON_POS);
                            loadingOverlayHome.IsVisible = false;
                            break;
                    } 
                    ((ListView)sender).SelectedItem = null;
                    loadingOverlayHome.IsVisible = false;
                }
            };
        }

        protected override void OnAppearing()
        {
            loadingOverlayHome.IsVisible = false;
            base.OnAppearing();
        }

        protected async void openBarCodeScaner(int successType){
            var expectedFormat =  ZXing.BarcodeFormat.CODE_93;//LOOKING FOR THESE TYPES OF BARCODES (CODE128 == IEMI)
            var opts = new ZXing.Mobile.MobileBarcodeScanningOptions
            {
                PossibleFormats = new List<ZXing.BarcodeFormat> { expectedFormat }
            };

            var scanner = new ZXing.Mobile.MobileBarcodeScanner
            {
                TopText = "Scan Asset Id Code",
                CancelButtonText = "Exit"
            };
            scanner.UseCustomOverlay = false;

            try
            {
                var result = await scanner.Scan(opts);
                if (result != null)
                {
                    var username = usernameKey ?? string.Empty;
                    var password = passKey ?? string.Empty;
                    var deviceAddress = uniqueDeviceAddress ?? string.Empty;
                    var toUsername = usernameKey ?? string.Empty;
                    var assetId = result.Text ?? string.Empty;
                    WebServiceResponse response = null;

                    switch(successType){
                        case 0://collect and assign EUS device from response
                            response = await ServiceClient.Instance.TransferDeviceToEUS(username, password, assetId);
                            if (response.ResponseCode != 200)
                            {
                                showResponseErrorAlert(response.ResponseString);
                                return;
                            }
                            await DisplayAlert(Constants.TRANSFER_TITLE, Constants.TRANSFER_CONTENT, Constants.BUTTON_POS);
                            break;
                        case 1:
                            //activateDevice must accept before transfering / disposing 
                            //For PM and EUS
                            response = await ServiceClient.Instance.ActivateDevice(assetId, /*deviceAddress,*/ username, password, isDisposalAccept);
                            if (response.ResponseCode != 200)
                            {
                                await DisplayAlert(string.Empty, response.ResponseString, Constants.BUTTON_POS);
                                loadingOverlayHome.IsVisible = false;
                                return;
                            }
                            await DisplayAlert(Constants.TRANSFER_TITLE, Constants.TRANSFER_CONTENT,Constants.BUTTON_POS);
                            break;
                        case 2:
                            //Deactivate device
                            //Only for EUS
                            //Completes the disposal transfer to PM
                            response = await ServiceClient.Instance.DeactivateDevice(/*deviceAddress*/assetId, username, password);
                            if (response.ResponseCode != 200)
                            {
                                showResponseErrorAlert(response.ResponseString);
                                return;
                            }
                            await DisplayAlert(Constants.DISPOSAL_TITLE, Constants.DISPOSAL_SUBMIT_CONTENT, Constants.BUTTON_POS);
                            break;
                        case 3:
                            //Dispose Device for PM only
                            response = await ServiceClient.Instance.DisposeDevice(/*deviceAddress*/assetId, username, password);

                            if (response.ResponseCode != 200)
                            {
                                showResponseErrorAlert(response.ResponseString);
                                return;
                            }
                            await DisplayAlert(Constants.DISPOSAL_TITLE, Constants.DISPOSAL_CONTENT, Constants.BUTTON_POS);
                            break;
                        default:
                            await DisplayAlert("Tapped", "Error in code" + " row was selected", Constants.BUTTON_POS);
                            break;

                    }
                    loadingOverlayHome.IsVisible = false;
                    scanner.Cancel();
                }else
                {
                    await Navigation.PopAsync();
                }
            }
            finally
            {
                scanner.Cancel();
            }
        }

        protected async void showResponseErrorAlert(string responseString){
            JObject errorMessage = JObject.Parse(responseString);
            await DisplayAlert(string.Empty, errorMessage["message"].ToString(), Constants.BUTTON_POS);
            loadingOverlayHome.IsVisible = false;
        }

        protected override bool OnBackButtonPressed()
        {
            if (Application.Current.MainPage.Navigation.NavigationStack.Count > 0)
            {
                DisplayLogoutAlert();
                return true;
               
            }else{
                return base.OnBackButtonPressed();
            }
        }

        //redirect ot the login
        public async void DisplayLogoutAlert()
        {
            logoutFlag = await DisplayAlert(Constants.LOGOUT_TITLE, Constants.LOGOUT_CONTENT, Constants.BUTTON_POS, Constants.BUTTON_CAN);
            if (logoutFlag)
            {
                Application.Current.MainPage = new NavigationPage(new AssetTrackPOCPage());
            }
        }
    }
}
