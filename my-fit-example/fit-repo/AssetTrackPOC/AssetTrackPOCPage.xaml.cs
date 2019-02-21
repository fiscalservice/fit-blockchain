/**
 * This is the main controller on start up of the application containing the login structure.
 * @type controller
 * @author David Amalfitano <damalfitano@deloitte.com>
 **/

using System;
using AssetTrackPOC.Controllers;
using AssetTrackPOC.Dashboard;
using AssetTrackPOC.Model;
using Nethereum.Hex.HexConvertors.Extensions;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using Xamarin.Forms;

namespace AssetTrackPOC
{
    public partial class AssetTrackPOCPage : ContentPage
    {
        private string TAG = "AssetTrackPOCPAGE.cs";

        /*
         * Initalizes the UI 
         */
        public AssetTrackPOCPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            Xamarin.Forms.Application.Current.Properties[Constants.CURRENT_DATE] = null;
            Xamarin.Forms.Application.Current.Properties[Constants.DEVICE_ADDRESS] = null;
            Xamarin.Forms.Application.Current.Properties[Constants.USERNAME_KEY] = null;
            Xamarin.Forms.Application.Current.Properties[Constants.PASS_KEY] = null;
            // CrossConnectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;
        }

        //When UI is displayed
        protected async override void OnAppearing()
        {
            base.OnAppearing();

            //Network check
            if (!CrossConnectivity.Current.IsConnected)
            {
                await DisplayAlert("Error", Constants.CHECK_CONNECTION, Constants.BUTTON_POS);
            }

            if (Constants.isTesting)
            {
                passwordText.Text = "password";

            }
        }

        //private async void Current_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        //{
        //    if (!e.IsConnected)
        //    {
        //        await DisplayAlert("Error", Constants.CHECK_CONNECTION, Constants.BUTTON_POS);
        //    }
        //}

        /*
         * On login button click event
         */
        async void OnButtonClicked(object sender, EventArgs args)
        {
            loadingOverlay.IsVisible = true;

            //perform login
            var username = usernameText.Text ?? string.Empty;
            var password = passwordText.Text ?? string.Empty;

            //Test mock Accounts
            UseMockDeviceAddress(username);

            //Checking for null values
            if (username.Length == 0 || password.Length == 0)
            {
                await DisplayAlert(string.Empty, Constants.INVALID_LOGIN, Constants.BUTTON_POS);
                loadingOverlay.IsVisible = false;
                return;
            }

            var assetId = Application.Current.Properties[Constants.DEVICE_ADDRESS] as string;
            Application.Current.Properties[Constants.USERNAME_KEY] = username;
            Application.Current.Properties[Constants.PASS_KEY] = password;

            //Network Check
            if (!CrossConnectivity.Current.IsConnected)
            {
                await DisplayAlert("Error", Constants.CHECK_CONNECTION, Constants.BUTTON_POS);
                loadingOverlay.IsVisible = false;
                return;
            }

            //Calling loginDevice service contract
            var response = await ServiceClient.Instance.LoginDevice(assetId, username, password);
                       
            if (response.ResponseCode != 200)
            {
                await DisplayAlert(string.Empty, Constants.INVALID_LOGIN, Constants.BUTTON_POS);
                loadingOverlay.IsVisible = false;
               return;
            }

            //Displaying error message from server
            if (response.ResponseString != null)
            {
                var responseObject = JObject.Parse(response.ResponseString);
                //Setting the user account type to toggle UI
                if (responseObject["message"] != null)
                {
                    await DisplayAlert("Alert!", Constants.UNAUTHORIZED_ACCESS, Constants.BUTTON_POS);
                    loadingOverlay.IsVisible = false;
                    return;
                }

                switch (username)
                {
                    case "eus1": // eus
                        LogInResponseData.SetApplicationCurrentProperty(Constants.ACCOUNT_TYPE, "1");
                        break;
                    case "pm1"://pm
                        LogInResponseData.SetApplicationCurrentProperty(Constants.ACCOUNT_TYPE, "0");
                        break;
                    default:
                        LogInResponseData.SetApplicationCurrentProperty(Constants.ACCOUNT_TYPE, (string)responseObject["roles"][0]);
                        break;
                }

                DateTime current = DateTime.Now;
                Application.Current.Properties[Constants.CURRENT_DATE] = current.ToString();
            }

            //good login, replain main controller
            loadingOverlay.IsVisible = false;
            Application.Current.MainPage = new NavigationPage(new HomePage());
        }

        //Grabbing the mock login accounts for POC
        private void UseMockDeviceAddress(string username){
            foreach (var item in Data.MockAccountsList)
            {
                if (username.Equals(item.username))
                {
                    Application.Current.Properties[Constants.DEVICE_ADDRESS] = (string)item.address;
                    Application.Current.Properties[Constants.USER_COSTCODE] = (string)item.costcode;
                    Application.Current.Properties[Constants.USER_LOCATION] = (string)item.location;
                }
            }
        }

      
        //NOT IN USE TODO: getDeviceAddress
        public async void getDeviceSignature()
        {
            var web3 = new Nethereum.Web3.Web3("http://ec2-52-91-125-113.compute-1.amazonaws.com:3000/");
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            //var account = new Nethereum.Accounts.Account(privateKey);
            System.Diagnostics.Debug.WriteLine("PLEASE********************************1");
            System.Diagnostics.Debug.WriteLine(TAG, web3.Personal.SignAndSendTransaction.GetHashCode().ToString());
            Application.Current.Properties["privateKey"] = privateKey;
            System.Diagnostics.Debug.WriteLine(TAG, "********************************22" + privateKey);


            System.Diagnostics.Debug.WriteLine(TAG, "********************************33" + Id);
            System.Diagnostics.Debug.WriteLine("PLEASE********************************");
            if (Application.Current.Properties.Count == 0)
            {
                // Device Signature
                var getDeviceAddress = await ServiceClient.Instance.getDeviceAddress();
                if (getDeviceAddress.ResponseString != null)
                {
                    var responseObject = JObject.Parse(getDeviceAddress.ResponseString);
                    Application.Current.Properties[Constants.DEVICE_ADDRESS] = (string)responseObject["address"];
                    Application.Current.Properties[Constants.PRIVATE_KEY] = (string)responseObject["privateKey"];
                }
                else
                {
                    await DisplayAlert("Error", Constants.UNAUTHORIZED_ACCESS, Constants.BUTTON_POS);
                }
                await Application.Current.SavePropertiesAsync();
            }
        }
    } //class
} //namespace
