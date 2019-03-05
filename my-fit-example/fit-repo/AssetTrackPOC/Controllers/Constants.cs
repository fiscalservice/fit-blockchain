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
        //Change to false for production
        public static bool isTesting = false;

        /*Service Calls*/
        public static string domain = "http://ec2-34-229-91-174.compute-1.amazonaws.com:3000/";
        public static string loginURL = "api/loginDevice";
        public static string ingestDeviceURL =  "api/ingestDevice";

        public static string transferDeviceURL =  "api/transferDevice";
        public static string transferDeviceToPmURL = "api/transferToPM";
        public static string collectDevice = "api/collectDeviceFromEmployee";

        public static string inventoryCheckURL =  "NULL";
        public static string disposeDeviceURL = "api/requestForDisposal";
        public static string deactivateDeviceURL = "api/disposeDevice";
        public static string acceptDeviceURL = "api/acceptDevice";
        public static string acceptDisposalURL = "api/acceptForDisposal";
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
        public static string ISACTIVE_PARAM = "isActive=";
        public static string PARAM_LOCATION = "location=WashingtnDC";

        public static string INCOMING_TYPE_KEY = "0";
        public static string OUTGOING_TYPE_KEY = "1";

        //User Property Keys
        public static string DEVICE_ADDRESS = "DEVICE_ADDRESS";
        public static string PRIVATE_KEY = "PRIVATE_KEY";
        public static string ACCOUNT_TYPE = "ACCOUNT_TYPE";
        public static string USERNAME_KEY = "USERNAME_KEY";
        public static string PASS_KEY = "PASS_KEY";
        public static string USER_COSTCODE = "USER_COSTCODE";
        public static string USER_LOCATION = "USER_LOCATION";
        public static string CURRENT_DATE = "CURRENT_DATE";
        public static string DEVICE_MODEL = "DEVICE_MODEL";

        //UI Text
        public static string BUTTON_POS = "OKAY";
        public static string BUTTON_CONFIRM = "CONFIRM";
        public static string BUTTON_CAN = "CANCEL";
        public static string BUTTON_RETURN = "RETURN HOME";
        public static string BUTTON_VIEW_TRANS = "ViEW TRANSFERS";
        public static string ERROR_TITLE = "Error";
        public static string LOGOUT_TITLE = "Logout";
        public static string TRANSFER_TITLE = "Transfer Complete";
        public static string TRANSFER_INITIATED = "Transfer Initiated";
        public static string DISPOSAL_TITLE = "Disposal";
        public static string INVENTORY_CHECK_TITLE = "Initiate Inventory Check";

        //Alert Content
        public static string CHECK_CONNECTION = "Check for your connection.";
        public static string INVALID_LOGIN = "Invalid Username or Password";
        public static string UNAUTHORIZED_ACCESS = "Unauthorized access attempt, please see closest support";
        public static string INVENTORY_CHECK_CONTENT = "By submitting theis inventory check, you are requesting that all employees within the Bureau verify their assets.";
        public static string TRANSFER_CONTENT = "Possesion of device is now assigned to your account. No further action is necessary.";
        public static string TRANSFER_INITITATED_CONTENT = "The device has been registered on the blockchain and the transfer has been initiated. " +
            "The recipient will need to log onto their app to accept the transfer.";
        public static string DISPOSAL_SUBMIT_CONTENT = "This device was submitted for disposal succesfully.";
        public static string DISPOSAL_CONTENT = "This device was disposed succesfully.";
        public static string LOGOUT_CONTENT = "If you continue you will be logged out of the application.";
        public static string FILL_ALL_INFO_CONTENT = "All information must be filled out to sumbit form";


        //Colors
        public static string PRIMARY_COLOR = "";
        public static string SECONDARY_COLOR = "";
        public static string ACCENT_COLOR = "#FF00BCD4";
        public static string LIST_HEADER_BG_COLOR = "#CFD8DC";
        public static string LIST_SEPERATOR_COLOR = "#f2f2f2";
        public static string TAB_BAR_COLOR = "#FF607D8B";
    }
}

