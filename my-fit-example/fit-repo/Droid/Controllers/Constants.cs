/**
 * This is the controller to handle Constants used throughtout application.
 * @type controller
 * @author David Amalfitano <damalfitano@deloitte.com>
 **/

using System;
using Xamarin.Forms;

namespace AssetTrackPOC.Controllers
{
    public class Constants
    {
        /*Service Calls*/
        public static string domain = "http://ec2-52-91-125-113.compute-1.amazonaws.com:3000/";
        public static string loginURL = "api/loginDevice";
        public static string ingestDeviceURL =  "api/ingestDevice";

        public static string transferDeviceURL =  "api/transferDevice";
        public static string transferDeviceToPmURL = "api/transferToPM";

        public static string inventoryCheckURL =  "NULL";
        public static string disposeDeviceURL = "api/requestForDisposal";
        public static string deactivateDeviceURL = "api/disposeDevice";
        public static string acceptDeviceURL = "api/acceptDevice";
        public static string pastTransfesURL =  "api/pastTransfers";
        public static string getDeviceURL = "api/myDevices";
        public static string pastLoginsURL = "api/pastLogins";
        public static string createDeviceAddress = "admin/createDeviceAccount";//use this until using web3J library
        public static string getAccountsURL = "admin/accounts";

        /*Get Parameters*/
        public static string PARAM_START = "?";
        public static string PARAM_CONNECTOR = "&";
        public static string TYPE_PARAM = "type=";
        public static string USERNAME_PARAM = "username=";


        //User Property Keys
        public static string DEVICE_ADDRESS = "DEVICE_ADDRESS";
        public static string PRIVATE_KEY = "PRIVATE_KEY";
        public static string ACCOUNT_TYPE = "ACCOUNT_TYPE";
        public static string USERNAME_KEY = "USERNAME_KEY";
        public static string PASS_KEY = "PASS_KEY";
        public static string USER_COSTCODE = "USER_COSTCODE";
        public static string USER_LOCATION = "USER_LOCATION";




    }
}

