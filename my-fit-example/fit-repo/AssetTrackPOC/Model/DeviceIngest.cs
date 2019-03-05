/**
 * Model contains observable collections for Ingest device UI setup
 * @namespace AssetTrackPOC.Model.DeviceIngest
 * @type Data Model
 * @author David Amalfitano <damalfitano@deloitte.com>
 **/

using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace AssetTrackPOC.Model
{
    public static class DeviceIngest
    {
        #region DeviceList
        public static ObservableCollection<DevicesIngestData> DeviceList = new ObservableCollection<DevicesIngestData>
            {
           // new DevicesIngestData {Title="AssetID", SubTitle="1234567", TopPadding= new Thickness(0,0,0,0), DeviceColor=Color.Gray, enableEntry = false},
           // new DevicesIngestData {Title="IMEI Barcode", SubTitle="Tap to scan IMEI barcode", TopPadding= new Thickness(0,0,0,0), DeviceColor=Color.Gray, enableEntry = false},
            new DevicesIngestData {Title="Location", SubTitle="", TopPadding= new Thickness(0,0,0,0), DeviceColor=Color.Gray, enableEntry = true},
            new DevicesIngestData {Title="CostCode", SubTitle="", TopPadding= new Thickness(0,0,0,0), DeviceColor=Color.Gray, enableEntry = true},
            new DevicesIngestData {Title="Status", SubTitle="Active", TopPadding= new Thickness(0,0,0,0), DeviceColor=Color.Gray, enableEntry = true}
            };
        #endregion

        #region DeviceListRecipient
        public static ObservableCollection<DevicesIngestData> DeviceListRecipient = new ObservableCollection<DevicesIngestData>
            {
            //new DevicesIngestData {Title="Recipient UserID", SubTitle="Input Recipient Name", TopPadding= new Thickness(0,0,0,0), DeviceColor=Color.Orange, enableEntry = true},
            new DevicesIngestData {Title="Recipient Location", SubTitle="Input Recipient Location", TopPadding= new Thickness(0,0,0,0), DeviceColor=Color.Orange, enableEntry = false},
            new DevicesIngestData {Title="Recipient CostCode", SubTitle="Input Recipient CostCode", TopPadding= new Thickness(0,0,0,0), DeviceColor=Color.Orange, enableEntry = false},
            };
        #endregion
    }
}

