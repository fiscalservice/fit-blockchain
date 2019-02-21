using System;
namespace AssetTrackPOC.Controllers
{
    public class DateFormatHelper
    {
        public DateFormatHelper(){}

        //Converting from Unix
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime FromUnixTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }
    }
}
