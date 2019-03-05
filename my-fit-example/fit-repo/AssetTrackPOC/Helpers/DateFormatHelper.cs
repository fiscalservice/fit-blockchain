/**
 * This is the controller for the DateFormatHelper
 * @namespace AssetTrackPOC.Helpers.DateFormatHelper
 * @type Helper
 * @author David Amalfitano<damalfitano@deloitte.com>
 */
using System;
namespace AssetTrackPOC.Controllers
{
    public class DateFormatHelper
    {
        public DateFormatHelper(){}

        //Converting from Unix
        public static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromUnixTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }
    }
}
