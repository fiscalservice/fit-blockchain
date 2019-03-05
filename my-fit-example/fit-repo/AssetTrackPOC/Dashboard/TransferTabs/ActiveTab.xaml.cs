/**
 * Tab controller containing Active Transfers
 * @type controller
 * @author David Amalfitano <damalfitano@deloitte.com>
 **/

using System;
using System.Collections.Generic;
using AssetTrackPOC.Controllers;
using AssetTrackPOC.CustomCell;
using AssetTrackPOC.Helpers;
using AssetTrackPOC.Model;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace AssetTrackPOC.Dashboard.TransferTabs
{
    public partial class ActiveTab : ContentPage
    {
        //Member variables
        List<MobileDevice> incomingTransferList = new List<MobileDevice>();
        List<MobileDevice> outgoingTransferList = new List<MobileDevice>();
        Label incomingLb, outgoingLb, noActiveOutgoingLbl, noActiveIncomingLbl;
        ListView incomingLv, outgoingLv;
        StackLayout incomingStack = new StackLayout();
        string usernameKey;

        //Initialization
        public ActiveTab()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            usernameKey = Application.Current.Properties[Constants.USERNAME_KEY] as string;
            pullActiveTransfers(Constants.INCOMING_TYPE_KEY, Constants.OUTGOING_TYPE_KEY, usernameKey);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            incomingLb = ObjectCreatorHelper.createLabel(10, 12, Color.Transparent, TextAlignment.Start, Color.Black,"INCOMING", new Thickness(0,0,0,0));
            outgoingLb = ObjectCreatorHelper.createLabel(10, 12, Color.Transparent, TextAlignment.Start, Color.Black, "OUTGOING", new Thickness(0, 16, 0, 0));

            noActiveOutgoingLbl = ObjectCreatorHelper.createLabel(13, 40, Color.White, TextAlignment.Center, Color.Black, "No active transfers.", new Thickness(0, 0, 0, 0));
            noActiveOutgoingLbl.VerticalTextAlignment = TextAlignment.Center;
            noActiveOutgoingLbl.HorizontalTextAlignment = TextAlignment.Center;
           
            noActiveIncomingLbl = ObjectCreatorHelper.createLabel(13, 40, Color.White, TextAlignment.Center, Color.Black, "No active transfers.", new Thickness(0, 0, 0, 0));
            noActiveIncomingLbl.VerticalTextAlignment = TextAlignment.Center;
            noActiveIncomingLbl.HorizontalTextAlignment = TextAlignment.Center;

            ObjectCreatorHelper.addToRootParent(activeRl, incomingLb, 8, 8, -16);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            incomingTransferList.Clear();
            outgoingTransferList.Clear();
        }

        public void setUpUI(){

            incomingStack.Spacing = 0;
            incomingStack.Children.Add(incomingLv);
            incomingStack.Children.Add(noActiveIncomingLbl);

            ObjectCreatorHelper.addChildToParent(activeRl, incomingStack, incomingLb, 0, incomingLb.Height, -16);
            ObjectCreatorHelper.addChildToParent(activeRl, outgoingLb, incomingStack, 0, outgoingLb.Height, 0);
            ObjectCreatorHelper.addChildToParent(activeRl, outgoingLv, outgoingLb, 0, outgoingLb.Height, 0);
            ObjectCreatorHelper.addChildToParent(activeRl, noActiveOutgoingLbl, outgoingLv, 0, noActiveOutgoingLbl.Height, 0);

            if (incomingTransferList.Count > 1)
            {
                incomingStack.Children.Remove(noActiveIncomingLbl);
            }

            if (outgoingTransferList.Count > 1)
            { 
                activeRl.Children.Remove(noActiveOutgoingLbl);
            }
        }


        public async void pullActiveTransfers(string incomingType, string outgoingType, string username){
           
            incomingLv = ObjectCreatorHelper.createListview(false, new DataTemplate(typeof(CustomTableCell)), 40, false, Color.White);
            incomingLv.Margin = new Thickness(0,0,16,0);
           
            outgoingLv = ObjectCreatorHelper.createListview(false, new DataTemplate(typeof(CustomTableCell)), 40, false, Color.White);
            outgoingLv.Margin = new Thickness(0, 0, 16, 0);

            //Pull Incoming Transfers
            var responseIncomingPull = await ServiceClient.Instance.PastTransfers(incomingType, username, "true");
            incomingTransferList.Add(new MobileDevice { ID = "AssetId", Name = "Sender UserID", UnitCost = "CostCode", DeviceColor = Color.FromHex(Constants.LIST_HEADER_BG_COLOR), OtherLabel = "TimeStamp" });
            outgoingTransferList.Add(new MobileDevice { ID = "AssetId", Name = "Recipient UserID", UnitCost = "CostCode", DeviceColor = Color.FromHex(Constants.LIST_HEADER_BG_COLOR), OtherLabel = "TimeStamp" });
            createList(responseIncomingPull, incomingLv, incomingTransferList);
            //End Incoming Transfer Pull

            //Pull outgoingTransfers
            var responseOutgoingPull = await ServiceClient.Instance.PastTransfers(outgoingType, username, "true");
            createList(responseOutgoingPull, outgoingLv, outgoingTransferList);
            //End Outgoing Transfer Pull
            setUpUI();
        }

        public void createList(WebServiceResponse _response, ListView _listView, List<MobileDevice>  _list){
         
            if (_response.ResponseCode == 200)
            {
                if (_response.ResponseString != null && _response.ResponseString != "")
                {
                    var responseArray = JArray.Parse(_response.ResponseString);

                    foreach (JObject o in responseArray.Children<JObject>())
                    {
                        DateTime timePull = (System.DateTime)o["timeStamp"];//DateFormatHelper.FromUnixTime((long)o["returnValues"]["time"]);
                        string timeStamp = timePull.ToLocalTime().ToString("M/dd/yy h:mm tt");
                        _list.Add(new MobileDevice { ID = (string)o["assetId"], Name = (string)o["toUsername"], UnitCost = (string)o["toCostCode"], DeviceColor = Color.White, OtherLabel = timeStamp });//Console.WriteLine("Value of i: {0}", i)
                    }
                }
            }
            //End Outgoing Transfer Pull
            _listView.ItemsSource = _list;//
        }
    }//class
}//namespace
