/**
 * This is the controller for the LogInResponseData
 * @namespace AssetTrackPOC.Model.LogInResponseData
 * @type Model
 * @author David Amalfitano<damalfitano@deloitte.com>
 */
using System.Collections.Generic;
using Xamarin.Forms;

namespace AssetTrackPOC.Model
{
    public class DeviceSignature {
        public string v { get; set; }
        public string r { get; set; }
        public string s { get; set; }
    }

    public class LogInResponseData // login to application
    {
        public static LogInResponseData Default = new LogInResponseData();
        public string name { get; set; }
        public string accountType { get; set; }

        public static object GetApplicationCurrentProperty(string propertyKey)
        {
            object retValue = null;
            IDictionary<string, object> properties = Application.Current.Properties;
            if (properties.ContainsKey(propertyKey))
            {
                retValue = properties[propertyKey];
            }
            return retValue;
        }
        public static void SetApplicationCurrentProperty(string propertyKey, object obj)
        {
            IDictionary<string, object> properties = Application.Current.Properties;
            if (properties.ContainsKey(propertyKey))
            {
                properties[propertyKey] = obj;
            }
            else
            {
                properties.Add(propertyKey, obj);
            }
        }
    }

    public class LogInDeviceResponseData //unlock device
    {
        public static LogInDeviceResponseData Default = new LogInDeviceResponseData();   
        public DeviceSignature deviceSignature {get; set;}
        public string fromAddress {get; set;}
        public string password { get; set; }
        public string accountType { get; set; }

        public static object GetApplicationCurrentProperty(string propertyKey){
            object retValue = null;
            IDictionary<string, object> properties = Application.Current.Properties;
            if (properties.ContainsKey(propertyKey))
            {
                retValue = properties[propertyKey];
            }
            return retValue;
        }
        public static void SetApplicationCurrentProperty(string propertyKey, object obj)
        {
            IDictionary<string, object> properties = Application.Current.Properties;
            if (properties.ContainsKey(propertyKey))
            {
                properties[propertyKey] = obj;
            }
            else
            {
                properties.Add(propertyKey, obj);
            }
        }
    }
}

