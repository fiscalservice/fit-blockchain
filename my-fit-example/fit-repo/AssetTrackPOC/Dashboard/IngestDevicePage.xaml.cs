/**
 * This is the controller is part of the dashboard is the Ingest device screen.
 * Prompts barcode scanner on init
 * @type controller
 * @author David Amalfitano <damalfitano@deloitte.com>
 **/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetTrackPOC.Controllers;
using AssetTrackPOC.CustomCell;
using AssetTrackPOC.Helpers;
using AssetTrackPOC.Model;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace AssetTrackPOC.Dashboard
{
    public partial class IngestDevicePage : ContentPage
    {
        //Member Variables
        Button submitBtn;
        List<AccountObject> accountList = new List<AccountObject>();
        ListView ingestDeviceRecipientLv, ingestDeviceLv;
        Label ImeiCodeRowSubtitle, AssetRowSubtitle;
        Entry recipientEntry;
        BoxView assetDivider;
        string usernameKey, passKey, costCode, location;
        string deviceModel = "Samsung";
        string recipientCostCode = null;
        Picker recipientPicker;

        public IngestDevicePage()
        {
            InitializeComponent();
            Title = "Ingest a New Device";
            init();
            deviceModel = GetDeviceModel();// Xamarin.Forms.Device.Android;//.Current.Model;
            usernameKey = Application.Current.Properties[Constants.USERNAME_KEY] as string;
            passKey = Application.Current.Properties[Constants.PASS_KEY] as string;
        }

        protected string GetDeviceModel(){
            string manufacturer = Device.Android +""as string;
            return manufacturer;
        }

        protected void init()
        {
            costCode = Application.Current.Properties[Constants.USER_COSTCODE] as string;
            location = Application.Current.Properties[Constants.USER_LOCATION] as string;
            List<DevicesIngestData> pmDeviceInfoList = new List<DevicesIngestData>();

            pmDeviceInfoList.Add(new DevicesIngestData { Title = "Location", SubTitle = location, TopPadding = new Thickness(0, 0, 0, 0), DeviceColor = Color.Gray, enableEntry = false });
            pmDeviceInfoList.Add(new DevicesIngestData { Title = "CostCode", SubTitle = costCode, TopPadding = new Thickness(0, 0, 0, 0), DeviceColor = Color.Gray, enableEntry = false });
            pmDeviceInfoList.Add(new DevicesIngestData { Title = "Status", SubTitle = "Active", TopPadding = new Thickness(0, 0, 0, 0), DeviceColor = Color.Gray, enableEntry = true });

            ingestDeviceLv = ObjectCreatorHelper.createListview(false, new DataTemplate(typeof(IngestDeviceCell)),80,true,Color.White);
            ingestDeviceLv.ItemsSource = pmDeviceInfoList;

            ingestDeviceRecipientLv = ObjectCreatorHelper.createListview(false, new DataTemplate(typeof(IngestDeviceCell)), 80, true, Color.White);
            ingestDeviceRecipientLv.ItemsSource = DeviceIngest.DeviceListRecipient;

            submitBtn = ObjectCreatorHelper.createbutton("Submit", 12, true, 55, Color.FromHex(Constants.ACCENT_COLOR), Color.White, FontAttributes.Bold, new Thickness(-5, -5, -5, 0));
            submitBtn.Clicked += OnSubmitButtonClicked;

            SetUpAssetIdUI();
            SetUpImeiUI();
            pullListOfAccounts();
            SetUpReceipientUI();
        }

        private void SetUpAssetIdUI()
        {
            /** Asset Row Set Up**/
            var AssetRowTitle = ObjectCreatorHelper.createLabel(12, 32, Color.White, TextAlignment.Center, Color.Gray, "AssetID", new Thickness(0, 0, 0, 0));
            AssetRowSubtitle = ObjectCreatorHelper.createLabel(14, 32, Color.White, TextAlignment.Center, Color.LightGray, "Tap to scan assetID barcode", new Thickness(0, 0, 0, 0));
            var AssetIdRow = ObjectCreatorHelper.createStackLayout(new Thickness(8, 0, 8, 0), Color.White, 80, true);
            AssetIdRow.Children.Add(AssetRowTitle);
            AssetIdRow.Children.Add(AssetRowSubtitle);
            AssetIdRow.GestureRecognizers.Add(new TapGestureRecognizer{Command = new Command(() => SetUpScanner(AssetRowSubtitle, ZXing.BarcodeFormat.CODE_93, "Scan Asset Id Code")),});

            ObjectCreatorHelper.addToRootParent(deviceInfoRl, AssetIdRow, 8, 8, 16);
            assetDivider = new BoxView { WidthRequest = 200, HeightRequest = 1, BackgroundColor = Color.LightGray };
            ObjectCreatorHelper.addChildToParent(deviceInfoRl, assetDivider, AssetIdRow, 0, assetDivider.Height, 0);
            /** END - Asset Row Set Up**/
        }

        private void SetUpImeiUI()
        {
            /** IMEI Row Set Up**/
            var ImeiCodeRowTitle = ObjectCreatorHelper.createLabel(12, 32, Color.White, TextAlignment.Center, Color.Gray, "IMEI Barcode", new Thickness(0, 0, 0, 0));
            ImeiCodeRowSubtitle = ObjectCreatorHelper.createLabel(14, 32, Color.White, TextAlignment.Center, Color.LightGray, "Tap to scan IMEI barcode", new Thickness(0, 0, 0, 0));
            var ImeiCodeRow = ObjectCreatorHelper.createStackLayout(new Thickness(8, 0, 8, 0), Color.White, 80, true);
            ImeiCodeRow.Children.Add(ImeiCodeRowTitle);
            ImeiCodeRow.Children.Add(ImeiCodeRowSubtitle);
            ImeiCodeRow.GestureRecognizers.Add(new TapGestureRecognizer{Command = new Command(() => SetUpScanner(ImeiCodeRowSubtitle, ZXing.BarcodeFormat.CODE_128, "Scan Asset IMEI Code")),});

            var imeiDivider = new BoxView { WidthRequest = 200, HeightRequest = 1, BackgroundColor = Color.LightGray };

            ObjectCreatorHelper.addChildToParent(deviceInfoRl, ImeiCodeRow, assetDivider, 0, assetDivider.Height+0.25, 0);
            ObjectCreatorHelper.addChildToParent(deviceInfoRl, imeiDivider, ImeiCodeRow, 0, ImeiCodeRow.Height, 0);
            ObjectCreatorHelper.addChildToParent(deviceInfoRl, ingestDeviceLv, imeiDivider, 0, ImeiCodeRow.Height+0.25, 0);
            /**END -  IMEI Row Set Up**/
        }

        private void SetUpReceipientUI(){
            /** Receipient Row Set Up**/
            var recipientRowTitle = ObjectCreatorHelper.createLabel(12, 22, Color.White, TextAlignment.Center, Color.Orange, "Recipient UserID", new Thickness(8, 4, 8, 0));

            //setUpRecipientPicker();

            recipientEntry = ObjectCreatorHelper.createEntry(14, 36, Color.White, Color.Gray, new Thickness(8, 8, 8, 0), "Input Recipient Username", Keyboard.Create(KeyboardFlags.CapitalizeSentence));
            var recipientDivider = new BoxView{WidthRequest = 200,HeightRequest = 0.25,BackgroundColor = Color.LightGray,Margin = new Thickness(0, 0, 0, 0)};
            var RecipientRow = ObjectCreatorHelper.createStackLayout(new Thickness(0, 8, 0, 0), Color.White, 80, true);
            RecipientRow.Children.Add(recipientRowTitle);
            RecipientRow.Children.Add(recipientEntry);
            RecipientRow.Children.Add(recipientDivider);

            ObjectCreatorHelper.addChildToParent(deviceInfoRl, RecipientRow, ingestDeviceLv, 0, ingestDeviceLv.Height+8, 0);
            var receipientDivider = new BoxView { WidthRequest = 200, HeightRequest = 1, BackgroundColor = Color.LightGray };
            ObjectCreatorHelper.addChildToParent(deviceInfoRl, receipientDivider, RecipientRow, 0, RecipientRow.Height+0.25, 0);
           recipientEntry.Completed += Entry_Completed;
          
            void Entry_Completed(object sender, EventArgs e)
            {
                autoPopulateEntry(((Entry)sender));
            }
            ObjectCreatorHelper.addChildToParent(deviceInfoRl, ingestDeviceRecipientLv, receipientDivider, 0, receipientDivider.Height+0.25, 0);
            ObjectCreatorHelper.addChildToParent(deviceInfoRl, submitBtn, ingestDeviceRecipientLv, 0, ingestDeviceLv.Height+8, 0);
            /** END - User Recipient Row Set Up**/
        }

        protected void setUpRecipientPicker(){

            // Uncomment for drop down list **KEEP**
            recipientPicker = new Picker
            {
                HeightRequest = 40,
                BackgroundColor = Color.White,
                Margin = new Thickness(),
                Title = "Select Recipient",
                VerticalOptions = LayoutOptions.Center
            };
            
          recipientPicker.SelectedIndexChanged += (sender, args) =>
          {
              if (recipientPicker.SelectedIndex != -1)
              {
                  string eusUserNameSelected = recipientPicker.Items[recipientPicker.SelectedIndex];
                  //autoPopulateEntry(eusUserNameSelected);
              }
          };

            /* for picker -- Add at end of PullListOfAccounts()
           *foreach (AccountObject eusAccount in accountList)
           *{
           *    if (eusAccount.role.Equals("1"))
           *    {
           *        recipientPicker.Items.Add(eusAccount.username);
           *    }
           }*/
        }

        protected async void SetUpScanner(Label labelToSet, ZXing.BarcodeFormat expectedFormat, string topText)
        {
            var scanner = new ZXing.Mobile.MobileBarcodeScanner
            {
                TopText = topText,//"Scan Asset Id Code",
                CancelButtonText="Exit"
            };

           var opts = new ZXing.Mobile.MobileBarcodeScanningOptions
            {
                PossibleFormats = new List<ZXing.BarcodeFormat> { expectedFormat }
            };

            try
            {
                scanner.AutoFocus();
                var result = await scanner.Scan(opts);
                scanner.CameraUnsupportedMessage = "camera Unsupported";
                if (result != null)
                {
                    labelToSet.Text = result.ToString();
                    labelToSet.TextColor = Color.Gray;
                    scanner.Cancel();
                }else{
                    scanner.Cancel();
                }

            }catch(Exception e){
                await DisplayAlert(Constants.ERROR_TITLE, e.Message, Constants.BUTTON_POS);
                scanner.Cancel();
            }finally
            {
                scanner.Cancel();
            }
        }

        private async void OnSubmitButtonClicked(object sender, EventArgs e)
        {
            //Retrieving Inputs
            var assetId = AssetRowSubtitle.Text ?? string.Empty;
            var imei = ImeiCodeRowSubtitle.Text ?? string.Empty;
            var receipientUsername = recipientEntry.Text ?? string.Empty;
            var password = passKey;
            var typeOfDevice = deviceModel;

            if (assetId.Equals("Tap to scan assetID barcode") || imei.Equals("Tap to scan IMEI barcode") || receipientUsername.Length == 0)
            {
                await DisplayAlert(string.Empty, Constants.FILL_ALL_INFO_CONTENT, Constants.BUTTON_POS);
                return;
            }
            submitBtn.IsEnabled = false;
            var response = await ServiceClient.Instance.CreateDevice(usernameKey, password, assetId, imei, typeOfDevice, costCode, receipientUsername, recipientCostCode);

            if (response.ResponseCode != 200)
            {
                JObject errorMessage = JObject.Parse(response.ResponseString);
                await DisplayAlert(string.Empty, errorMessage["message"].ToString(), Constants.BUTTON_POS);
                submitBtn.IsEnabled = true;
                return;
            }
            else
            {
                var result = await DisplayAlert(Constants.TRANSFER_INITIATED,Constants.TRANSFER_INITITATED_CONTENT, Constants.BUTTON_RETURN, Constants.BUTTON_VIEW_TRANS);

                if (result == false) // view transfers btn pressed
                {
                    await Navigation.PushAsync(new InitiateTransferPage());
                }
                else // return home btn pressed
                {
                    await Navigation.PopAsync();
                }
            }
        }

        private async void pullListOfAccounts(){
            
            var response = await ServiceClient.Instance.GetAccounts();
            if (response.ResponseCode != 200)
            {
                await DisplayAlert(string.Empty, Constants.CHECK_CONNECTION, Constants.BUTTON_POS);
                submitBtn.IsEnabled = true;
                return;
            }

            if (response.ResponseString != null)
            {
                var responseArray = JArray.Parse(response.ResponseString);

                if (responseArray.Count > 0) { 
                    foreach (JObject o in responseArray.Children<JObject>())
                    {
                        JArray roleArray = (Newtonsoft.Json.Linq.JArray)o["roles"];
                        string role = roleArray.First.ToString() ;//roleArray +"";
                        accountList.Add(new AccountObject() {username = (string)o["username"], location = (string)o["location"], costCode = (string)o["costCode"], address = (string)o["address"], role = role});
                    }
                }
            }
        }
       
        //AutoPopulating the recipient information from pulled Account list
        private void autoPopulateEntry(Entry eusSelected){
            List<DevicesIngestData> receipientAutoFill = new List<DevicesIngestData>();
            foreach (var item in accountList)
            {
                if(eusSelected.Text.ToString().Equals(item.username)){
                    receipientAutoFill.Add(new DevicesIngestData { Title = "Recipient Location", SubTitle = item.location, TopPadding = new Thickness(0, 0, 0, 0), DeviceColor = Color.Orange, enableEntry = false });
                    receipientAutoFill.Add(new DevicesIngestData {Title="Recipient CostCode", SubTitle=item.costCode, TopPadding= new Thickness(0,0,0,0), DeviceColor=Color.Orange, enableEntry = false});
                    recipientCostCode = item.costCode;
                    ingestDeviceRecipientLv.BeginRefresh();
                    ingestDeviceRecipientLv.ItemsSource = receipientAutoFill;
                    ingestDeviceRecipientLv.EndRefresh();
                    eusSelected.TextColor = Color.Gray;
                }
            }
        }
    }
}
