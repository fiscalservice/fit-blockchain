///*CURRENTLY NOT USING*/

//using System;
////using System.Collections.Generic;
////using System.Linq;
////using System.Text;
//using System.Threading.Tasks;
//using Xamarin.Forms;
////using ZXing.Net.Mobile.Forms;

//using System.Collections.Generic;
//using AssetTrackPOC.Controllers;
//using AssetTrackPOC.Model;

//namespace AssetTrackPOC.Dashboard.BarcodeScaner
//{
   

//    public class CustomScanPage
//    {
//        public static WebServiceResponse responseFromScanner = null;


//        public static WebServiceResponse OpenScanner(int successType)
//        {

//            openBarCodeScaner(successType);

//            return responseFromScanner;
//        }

//        protected static async void openBarCodeScaner(int successType)
//        {
//            var expectedFormat = ZXing.BarcodeFormat.CODE_39 | ZXing.BarcodeFormat.CODE_93;//LOOKING FOR THESE TYPES OF BARCODES (CODE128 == IEMI)
//            var opts = new ZXing.Mobile.MobileBarcodeScanningOptions
//            {
//                PossibleFormats = new List<ZXing.BarcodeFormat> { expectedFormat }
//            };

//            var scanner = new ZXing.Mobile.MobileBarcodeScanner();
//            //scanner.CancelButtonText = "CANCEL"; // use for custom overloay
//            scanner.UseCustomOverlay = false;

//            try
//            {
//                var result = await scanner.Scan(opts);
//                if (result != null)
//                {
//                    var username = "Mobile1";//usernameText.Text ?? string.Empty;
//                    var password = "Password#1";//passwordText.Text ?? string.Empty;
//                    var deviceAddress = "0x461dE94b2fAdbb57c8DE4976fa40233F9e9D1744";
//                    var toUsername = "Harold";
//                    var assetId = result.Text;
//                    WebServiceResponse response = null;

//                    switch (successType)
//                    {
//                        case 0:

//                            //transferDevice -- REMOVED
//                            response = await ServiceClient.Instance.TransferDevice(username, password, toUsername, assetId);
//                            responseFromScanner = response;
//                            //if (response.ResponseCode != 200)
//                            //{
//                            //    await DisplayAlert(string.Empty, "Error has occurred", "Ok");
//                            //    //loadingOverlay.IsVisible = false;
//                            //    return;
//                            //}
//                            //await DisplayAlert("Transfer Initiated", "The device has been registered on the blockchain and the transfer has been initiated. The recipient will need to log onto their app to accept the transfer.", "OKAY");
//                            break;
//                        case 1:

//                            //activateDevice
//                            response = await ServiceClient.Instance.ActivateDevice(assetId,/* deviceAddress,*/ username, password);
//                            responseFromScanner = response;
//                            //if (response.ResponseCode != 200)
//                            //{
//                            //    await DisplayAlert(string.Empty, "Error has occurred", "Ok");
//                            //    //loadingOverlay.IsVisible = false;
//                            //    return;
//                            //}

//                            //await DisplayAlert("Transfer Complete", "Transfer from usersName complete. No further action is necessary.", "OKAY");
//                            break;
//                        case 2:

//                            //Deactivate device
//                            response = await ServiceClient.Instance.DeactivateDevice(assetId, username, password);
//                            responseFromScanner = response;
//                            if (response.ResponseCode != 200)
//                            {
//                                //await DisplayAlert(string.Empty, "Error has occurred", "Ok");
//                                //loadingOverlay.IsVisible = false;
//                                return;
//                            }

//                            //await DisplayAlert("Disposal", "This device was disposed succesfully.", "OKAY");
//                            break;
//                        case 3:

//                            //Dispose Device
//                            response = await ServiceClient.Instance.DisposeDevice(assetId, username, password);
//                            responseFromScanner = response;
//                            //if (response.ResponseCode != 200)
//                            //{
//                            //    await DisplayAlert(string.Empty, "Error has occurred", "Ok");
//                            //    //loadingOverlay.IsVisible = false;
//                            //    return;
//                            //}

//                            //await DisplayAlert("Disposal", "This device was submitted for disposal.", "OKAY");
//                            break;
//                        default:
//                            responseFromScanner = response;;
//                            //await DisplayAlert("Tapped", "Error in code" + " row was selected", "OKAY");
//                            break;
//                    }

//                    scanner.Cancel();
//                }
//                //else TODO
//                //{
//                //    await Navigation.PopAsync();
//                //}
//            }
//            finally
//            {
//                scanner.Cancel();
//            }
//        }

//    }

//}

